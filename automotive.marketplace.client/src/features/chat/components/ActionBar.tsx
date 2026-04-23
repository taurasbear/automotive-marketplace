import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
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
  buyerHasLiked: boolean;
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
  buyerHasLiked,
  hasActiveOffer,
  hasActiveMeeting,
  onSendOffer,
  onProposeMeeting,
  onShareAvailability,
}: ActionBarProps) => {
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [meetupPopoverOpen, setMeetupPopoverOpen] = useState(false);
  const [proposeMeetingOpen, setProposeMeetingOpen] = useState(false);
  const [shareAvailabilityOpen, setShareAvailabilityOpen] = useState(false);

  const isBuyer = currentUserId === buyerId;
  const isSeller = currentUserId === sellerId;
  const showButtons = isBuyer || (isSeller && buyerHasLiked);

  if (!showButtons) return null;

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        disabled={hasActiveOffer}
        onClick={() => setOfferModalOpen(true)}
        title={
          hasActiveOffer
            ? "An offer is already pending in this conversation"
            : undefined
        }
        className="shrink-0"
      >
        Make an Offer
      </Button>

      <Popover open={meetupPopoverOpen} onOpenChange={setMeetupPopoverOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            disabled={hasActiveMeeting}
            title={
              hasActiveMeeting
                ? "A meetup negotiation is already active"
                : undefined
            }
            className="shrink-0"
          >
            Plan Meetup 📅 ▾
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-48 p-1" align="start">
          <button
            className="hover:bg-muted w-full rounded-md px-3 py-2 text-left text-sm"
            onClick={() => {
              setMeetupPopoverOpen(false);
              setProposeMeetingOpen(true);
            }}
          >
            🗓️ Propose a time
          </button>
          <button
            className="hover:bg-muted w-full rounded-md px-3 py-2 text-left text-sm"
            onClick={() => {
              setMeetupPopoverOpen(false);
              setShareAvailabilityOpen(true);
            }}
          >
            ⏰ Share availability
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
