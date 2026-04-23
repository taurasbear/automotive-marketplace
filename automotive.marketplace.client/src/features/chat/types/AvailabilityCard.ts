export type AvailabilityCardStatus = 'Pending' | 'Responded' | 'Expired';

export type AvailabilitySlot = {
  id: string;
  startTime: string;
  endTime: string;
};

export type AvailabilityCard = {
  id: string;
  status: AvailabilityCardStatus;
  expiresAt: string;
  initiatorId: string;
  slots: AvailabilitySlot[];
};
