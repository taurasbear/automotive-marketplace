export type ContractCardStatus =
  | "Pending"
  | "Active"
  | "SellerSubmitted"
  | "BuyerSubmitted"
  | "Complete"
  | "Declined"
  | "Cancelled";

export type ContractCard = {
  id: string;
  status: ContractCardStatus;
  initiatorId: string;
  acceptedAt: string | null;
  createdAt: string;
  sellerSubmittedAt: string | null;
  buyerSubmittedAt: string | null;
};
