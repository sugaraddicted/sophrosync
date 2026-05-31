export interface ProfileDto {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface NotificationPreferenceDto {
  id: string;
  tenantId: string;
  userId: string | null;
  preferredChannel: string;
  emailEnabled: boolean;
  inAppEnabled: boolean;
  smsEnabled: boolean;
  emailAddress: string | null;
}
