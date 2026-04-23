import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { format } from "date-fns";
import { Calendar, Clock, DollarSign, Plus } from "lucide-react";
import { useState } from "react";
import type { Meeting } from "../types/Meeting";
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
  acceptedMeeting: Meeting | null;
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
  onCancelMeeting: (meetingId: string) => void;
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
  acceptedMeeting,
  onSendOffer,
  onProposeMeeting,
  onShareAvailability,
  onCancelMeeting,
}: ActionBarProps) => {
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [actionsPopoverOpen, setActionsPopoverOpen] = useState(false);
  const [proposeMeetingOpen, setProposeMeetingOpen] = useState(false);
  const [shareAvailabilityOpen, setShareAvailabilityOpen] = useState(false);
  const [guardAction, setGuardAction] = useState<
    "propose" | "availability" | null
  >(null);

  const isBuyer = currentUserId === buyerId;
  const isSeller = currentUserId === sellerId;
  const showButtons = isBuyer || (isSeller && buyerHasEngaged);

  if (!showButtons) return null;

  const handleMeetingAction = (action: "propose" | "availability") => {
    setActionsPopoverOpen(false);
    if (acceptedMeeting) {
      setGuardAction(action);
    } else if (action === "propose") {
      setProposeMeetingOpen(true);
    } else {
      setShareAvailabilityOpen(true);
    }
  };

  const handleGuardConfirm = () => {
    if (!acceptedMeeting || !guardAction) return;
    onCancelMeeting(acceptedMeeting.id);
    if (guardAction === "propose") {
      setProposeMeetingOpen(true);
    } else {
      setShareAvailabilityOpen(true);
    }
    setGuardAction(null);
  };

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
            onClick={() => handleMeetingAction("propose")}
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
            onClick={() => handleMeetingAction("availability")}
          >
            <Clock className="mr-2 h-4 w-4" />
            Share availability
          </button>
        </PopoverContent>
      </Popover>

      <AlertDialog
        open={guardAction !== null}
        onOpenChange={(open) => {
          if (!open) setGuardAction(null);
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cancel existing meetup?</AlertDialogTitle>
            <AlertDialogDescription>
              {acceptedMeeting
                ? `You have a confirmed meetup on ${format(new Date(acceptedMeeting.proposedAt), "EEE, MMM d")} at ${format(new Date(acceptedMeeting.proposedAt), "HH:mm")}. Starting a new negotiation will cancel it.`
                : "Starting a new negotiation will cancel the existing meetup."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Keep existing</AlertDialogCancel>
            <AlertDialogAction
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              onClick={handleGuardConfirm}
            >
              Continue
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

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
