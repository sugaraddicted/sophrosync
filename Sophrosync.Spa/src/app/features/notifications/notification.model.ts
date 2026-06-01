export interface NotificationDto {
  id: string;
  tenantId: string;
  recipientUserId: string;
  channel: string;
  type: string;
  subject: string;
  body: string;
  status: string;
  scheduledFor: string;
  sentAt: string | null;
  dismissedAt: string | null;
  createdAt: string;
}

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface UnreadCountResponse {
  Count: number; // capital C — matches C# serialization
}
