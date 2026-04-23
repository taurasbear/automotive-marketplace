import type { Meeting } from './Meeting';
import type { AvailabilityCard } from './AvailabilityCard';

export type MeetingProposedPayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  meeting: Meeting;
};

export type MeetingStatusUpdatedPayload = {
  meetingId: string;
  conversationId: string;
  newStatus: 'Accepted' | 'Declined';
  initiatorId: string;
  responderId: string;
  rescheduledMeeting: null;
};

export type MeetingRescheduledPayload = {
  meetingId: string;
  conversationId: string;
  newStatus: 'Rescheduled';
  initiatorId: string;
  responderId: string;
  rescheduledMeeting: MeetingProposedPayload;
};

export type MeetingExpiredPayload = {
  meetingId: string;
  conversationId: string;
};

export type AvailabilitySharedPayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  availabilityCard: AvailabilityCard;
};

export type AvailabilityRespondedPayload = {
  availabilityCardId: string;
  conversationId: string;
  action: 'PickSlot' | 'ShareBack';
  pickedSlotMeeting: MeetingProposedPayload | null;
  sharedBackAvailability: AvailabilitySharedPayload | null;
};

export type AvailabilityExpiredPayload = {
  availabilityCardId: string;
  conversationId: string;
};
