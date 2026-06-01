export type ConsentPurpose =
  | 'Treatment'
  | 'DataProcessing'
  | 'Marketing'
  | 'Research'
  | 'ThirdPartySharing';

export type ConsentTemplateStatus = 'Draft' | 'Published' | 'Retired';
export type ConsentRequestStatus = 'Pending' | 'Completed' | 'Expired' | 'Revoked';
export type ConsentAction = 'Granted' | 'Withdrawn';

export interface ConsentTemplateDto {
  id: string;
  tenantId: string;
  purpose: ConsentPurpose;
  title: string;
  bodyText: string;
  versionNumber: number;
  status: ConsentTemplateStatus;
  publishedAt: string | null;
  createdAt: string;
}

export interface ConsentRequestDto {
  id: string;
  tenantId: string;
  clientId: string;
  consentTemplateId: string;
  status: ConsentRequestStatus;
  expiresAt: string;
  completedAt: string | null;
  createdAt: string;
}

export interface ConsentRecordDto {
  id: string;
  tenantId: string;
  clientId: string;
  consentRequestId: string;
  purpose: ConsentPurpose;
  action: ConsentAction;
  templateVersion: number;
  createdAt: string;
}

export interface IssueConsentRequestPayload {
  tenantId: string;
  clientId: string;
  consentTemplateId: string;
  expiresAt: string;
}
