import { isNullOrUndefined } from "util";
import { IConfigurationProvider } from "azure-devops-emailreporttask/config/IConfigurationProvider";
import { PipelineConfiguration } from "azure-devops-emailreporttask/config/pipeline/PipelineConfiguration";
import { MailConfiguration } from "azure-devops-emailreporttask/config/mail/MailConfiguration";
import { ReportDataConfiguration } from "azure-devops-emailreporttask/config/report/ReportDataConfiguration";
import { SendMailCondition } from "azure-devops-emailreporttask/config/report/SendMailCondition";
import { PipelineType } from "azure-devops-emailreporttask/config/pipeline/PipelineType";
import { SmtpConfiguration } from "azure-devops-emailreporttask/config/mail/SmtpConfiguration";
import { StringUtils } from "azure-devops-emailreporttask/utils/StringUtils";
import { GroupTestResultsBy } from "azure-devops-emailreporttask/config/report/GroupTestResultsBy";
import { TestResultsConfiguration } from "azure-devops-emailreporttask/config/report/TestResultsConfiguration";
import { RecipientsConfiguration } from "azure-devops-emailreporttask/config/mail/RecipientsConfiguration";

export class JsonConfigProvider implements IConfigurationProvider {

  private jsonRequest: any;
  private pipelineConfiguration: PipelineConfiguration;
  private mailConfiguration: MailConfiguration;
  private reportDataConfiguration: ReportDataConfiguration;
  private sendMailCondition: SendMailCondition;

  constructor(jsonRequest: any, smtpConfig: SmtpConfiguration) {
    this.jsonRequest = jsonRequest;
    this.initPipelineConfiguration();
    this.initMailConfiguration(smtpConfig);
    this.initReportDataConfiguration();
  }

  getPipelineConfiguration(): PipelineConfiguration {
    return this.pipelineConfiguration;
  }
  getMailConfiguration(): MailConfiguration {
    return this.mailConfiguration;
  }
  getReportDataConfiguration(): ReportDataConfiguration {
    return this.reportDataConfiguration;
  }
  getSendMailCondition(): SendMailCondition {
    return this.sendMailCondition;
  }

  /**
   * Gets access token from system
   */
  private getAccessKey(): string {
    if (isNullOrUndefined(this.jsonRequest["System.AccessToken"])) {
      throw new Error("Invalid JSON Request. Agent AccessToken not provided.");
    }
    return this.jsonRequest["System.AccessToken"];
  }

  private initPipelineConfiguration(): void {
    if (isNullOrUndefined(this.jsonRequest.PipelineInfo)) {
      throw new Error("Invalid JSON Request. PipelineInfo not provided.");
    }
    const pipelineInfo = this.jsonRequest.PipelineInfo;

    const pipelineType = pipelineInfo.PipelineType == "build" ? PipelineType.Build : PipelineType.Release;
    const pipelineId = Number(pipelineInfo.Id);

    const projectId = pipelineInfo.ProjectId;
    const projectName = pipelineInfo.ProjectName;

    const envId = Number(pipelineInfo.EnvironmentId);
    const envDefId = Number(pipelineInfo.DefinitionEnvironmentId);

    const usePrevEnvironment = Boolean(pipelineInfo.UsePreviousEnvironment);
    const teamUri = pipelineInfo.ServerUri;
    this.pipelineConfiguration = new PipelineConfiguration(pipelineType, pipelineId, projectId, projectName, envId, envDefId, usePrevEnvironment, teamUri, this.getAccessKey());
  }

  private initMailConfiguration(smtpConfig: SmtpConfiguration): void {
    if (isNullOrUndefined(this.jsonRequest.EmailConfiguration)) {
      throw new Error("Invalid JSON Request. EmailConfiguration not provided.");
    }

    const emailConfig = this.jsonRequest.EmailConfiguration[0];

    // Mail Subject
    const mailSubject = emailConfig.MailSubject;
    if (StringUtils.isNullOrWhiteSpace(mailSubject)) {
      throw new Error("Email subject not set");
    }

    // Optional inputs
    const toAddresses = emailConfig.ToAddresses;
    const ccAddresses = emailConfig.CcAddresses;
    const includeInToAddressesConfig = emailConfig.IncludeInTo;
    const includeInCCAddressesConfig = emailConfig.IncludeInCc;

    // Addresses Configuration
    const toRecipientsConfiguration = this.getRecipientConfiguration(toAddresses, includeInToAddressesConfig);
    const ccRecipientsConfiguration = this.getRecipientConfiguration(ccAddresses, includeInCCAddressesConfig);

    this.mailConfiguration = new MailConfiguration(mailSubject, toRecipientsConfiguration, ccRecipientsConfiguration, smtpConfig, "@microsoft.com");
    this.initSendMailCondition(emailConfig);
  }

  private initReportDataConfiguration(): void {
    if (isNullOrUndefined(this.jsonRequest.ReportDataConfiguration)) {
      throw new Error("Invalid JSON Request. EmailConfiguration not provided.");
    }
    const reportDataConfig = this.jsonRequest.ReportDataConfiguration;
    // required inputs
    const groupResultsBy = this.getGroupTestResultsByEnumFromString(reportDataConfig.GroupTestResultsBy);
    const includeOthersInTotal = reportDataConfig.IncludeOthersInTotal;
    const maxTestFailuresToShow = Number(reportDataConfig.MaxFailuresToShow);
    const includeCommits = Boolean(reportDataConfig.IncludeCommits);

    // optional inputs
    const includeResultsStr = reportDataConfig.IncludeResults;
    const groupTestSummaryByStr = reportDataConfig.GroupTestSummaryBy;

    const groupTestSummaryBy: Array<GroupTestResultsBy> = new Array();
    if (groupTestSummaryByStr != null) {
      groupTestSummaryByStr.split(",").forEach((element: string) => { groupTestSummaryBy.push(this.getGroupTestResultsByEnumFromString(element)) });
    }

    // derived input values
    const includeResultsConfig = includeResultsStr == null ? includeResultsStr.split(",") : [];
    const includeFailedTests = includeResultsConfig.includes("1");
    const includeOtherTests = includeResultsConfig.includes("2");
    const includePassedTests = includeResultsConfig.includes("3");
    const includeInconclusiveTests = includeResultsConfig.includes("4");
    const includeNotExecutedTests = includeResultsConfig.includes("5");

    const testResultsConfig = new TestResultsConfiguration(includeFailedTests, includePassedTests, includeInconclusiveTests, includeNotExecutedTests, includeOtherTests, groupResultsBy, maxTestFailuresToShow);

    this.reportDataConfiguration = new ReportDataConfiguration(includeCommits, includeOthersInTotal, true, groupTestSummaryBy, testResultsConfig);
  }

  initSendMailCondition(emailConfig: any): void {
    const sendMailConditionStr = emailConfig.SendMailCondition;
    let sendMailCondition: SendMailCondition;
    switch (sendMailConditionStr) {
      case "On Failure": sendMailCondition = SendMailCondition.OnFailure; break;
      case "On Success": sendMailCondition = SendMailCondition.OnSuccess; break;
      case "On New Failures Only": sendMailCondition = SendMailCondition.OnNewFailuresOnly; break;
      default: sendMailCondition = SendMailCondition.Always; break;
    }
    this.sendMailCondition = sendMailCondition;
  }

  private getRecipientConfiguration(namedRecipients: string, includeConfigStr: string): RecipientsConfiguration {
    if (includeConfigStr != null) {
      const includeConfig = includeConfigStr.split(",");
      const includeChangesetOwners = includeConfig.includes("1");
      const includeTestOwners = includeConfig.includes("2");
      const includeActiveBugOwners = includeConfig.includes("3");
      const includeCreatedBy = includeConfig.includes("4");

      return new RecipientsConfiguration(namedRecipients, includeChangesetOwners, includeTestOwners, includeActiveBugOwners, includeCreatedBy);
    }

    return new RecipientsConfiguration(namedRecipients);
  }

  private getGroupTestResultsByEnumFromString(groupResultsByStr: string): GroupTestResultsBy {
    switch (groupResultsByStr) {
      case "Priority": return GroupTestResultsBy.Priority;
      case "Team": return GroupTestResultsBy.Team;
      default: return GroupTestResultsBy.Run;
    }
  }
}