export const HUB_METHODS = {
  // Client → Server
  SEND_MESSAGE: 'SendMessage',
  MAKE_OFFER: 'MakeOffer',
  RESPOND_TO_OFFER: 'RespondToOffer',
  // Server → Client
  RECEIVE_MESSAGE: 'ReceiveMessage',
  UPDATE_UNREAD_COUNT: 'UpdateUnreadCount',
  OFFER_MADE: 'OfferMade',
  OFFER_ACCEPTED: 'OfferAccepted',
  OFFER_DECLINED: 'OfferDeclined',
  OFFER_COUNTERED: 'OfferCountered',
  OFFER_EXPIRED: 'OfferExpired',
} as const;
