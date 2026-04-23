export type MeetingStatus = 'Pending' | 'Accepted' | 'Declined' | 'Rescheduled' | 'Expired';

export type Meeting = {
  id: string;
  proposedAt: string;
  durationMinutes: number;
  locationText?: string;
  locationLat?: number;
  locationLng?: number;
  status: MeetingStatus;
  expiresAt: string;
  initiatorId: string;
  parentMeetingId?: string;
};
