export interface ProfileDto {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface PracticeTargets {
  weeklySessionTarget: number;
  monthlySessionTarget: number;
}

export const DEFAULT_PRACTICE_TARGETS: PracticeTargets = {
  weeklySessionTarget: 5,
  monthlySessionTarget: 20,
};

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
