import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Calendar, Clock, DollarSign, Plus } from "lucide-react";
import { useState } from "react";
import MakeOfferModal from "./MakeOfferModal";
import ProposeMeetingModal from "./ProposeMeetingModal";
import ShareAvailabilityModal from "./ShareAvailabilityModal";

type ActionBarProps = {
  currentUserId: string;
  buyerId: string;
  sellerId: string;
  listingPrice: number;
  conversationId: string;
  buyerHasEngaged: boolean;
  hasActiveOffer: boolean;
  hasActiveMeeting: boolean;
  onSendOffer: (amount: number) => void;
  onProposeMeeting: (data: {
    proposedAt: string;
    durationMinutes: number;
    locationText?: string;
    locationLat?: number;
    locationLng?: number;
  }) => void;
  onShareAvailability: (
    slots: { startTime: string; endTime: string }[],
  ) => void;
};

const ActionBar = ({
  currentUserId,
  buyerId,
  sellerId,
  listingPrice,
  conversationId,
  buyerHasEngaged,
  hasActiveOffer,
  hasActiveMeeting,
  onSendOffer,
  onProposeMeeting,
  onShareAvailability,
}: ActionBarProps) => {
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [actionsPopoverOpen, setActionsPopoverOpen] = useState(false);
  const [proposeMeetingOpen, setProposeMeetingOpen] = useState(false);
  const [shareAvailabilityOpen, setShareAvailabilityOpen] = useState(false);

  const isBuyer = currentUserId === buyerId;
  const isSeller = currentUserId === sellerId;
  const showButtons = isBuyer || (isSeller && buyerHasEngaged);

  if (!showButtons) return null;

  return (
    <>
      <Popover open={actionsPopoverOpen} onOpenChange={setActionsPopoverOpen}>
        <PopoverTrigger asChild>
          <Button variant="outline" size="icon" className="h-8 w-8 shrink-0">
            <Plus className="h-4 w-4" />
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-48 p-1" align="start">
          <button
            className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm disabled:cursor-not-allowed disabled:opacity-50"
            disabled={hasActiveOffer}
            title={
              hasActiveOffer
                ? "An offer is already pending in this conversation"
                : undefined
            }
            onClick={() => {
              setActionsPopoverOpen(false);
              setOfferModalOpen(true);
            }}
          >
            <DollarSign className="mr-2 h-4 w-4" />
            Make an Offer
          </button>
          <button
            className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm disabled:cursor-not-allowed disabled:opacity-50"
            disabled={hasActiveMeeting}
            title={
              hasActiveMeeting
                ? "A meetup negotiation is already active"
                : undefined
            }
            onClick={() => {
              setActionsPopoverOpen(false);
              setProposeMeetingOpen(true);
            }}
          >
            <Calendar className="mr-2 h-4 w-4" />
            Propose a time
          </button>
          <button
            className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm disabled:cursor-not-allowed disabled:opacity-50"
            disabled={hasActiveMeeting}
            title={
              hasActiveMeeting
                ? "A meetup negotiation is already active"
                : undefined
            }
            onClick={() => {
              setActionsPopoverOpen(false);
              setShareAvailabilityOpen(true);
            }}
          >
            <Clock className="mr-2 h-4 w-4" />
            Share availability
          </button>
        </PopoverContent>
      </Popover>

      <MakeOfferModal
        open={offerModalOpen}
        onOpenChange={setOfferModalOpen}
        mode="offer"
        listingPrice={listingPrice}
        conversationId={conversationId}
        onSubmit={(amount) => {
          onSendOffer(amount);
          setOfferModalOpen(false);
        }}
      />

      <ProposeMeetingModal
        open={proposeMeetingOpen}
        onOpenChange={setProposeMeetingOpen}
        mode="propose"
        onSubmit={(data) => {
          onProposeMeeting(data);
          setProposeMeetingOpen(false);
        }}
      />

      <ShareAvailabilityModal
        open={shareAvailabilityOpen}
        onOpenChange={setShareAvailabilityOpen}
        onSubmit={(slots) => {
          onShareAvailability(slots);
          setShareAvailabilityOpen(false);
        }}
      />
    </>
  );
};

export default ActionBar;
