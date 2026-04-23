import { Button } from '@/components/ui/button';
import { useState } from 'react';
import MakeOfferModal from './MakeOfferModal';

type ActionBarProps = {
  currentUserId: string;
  buyerId: string;
  sellerId: string;
  listingPrice: number;
  conversationId: string;
  buyerHasLiked: boolean;
  hasActiveOffer: boolean;
  onSendOffer: (amount: number) => void;
};

const ActionBar = ({
  currentUserId,
  buyerId,
  sellerId,
  listingPrice,
  conversationId,
  buyerHasLiked,
  hasActiveOffer,
  onSendOffer,
}: ActionBarProps) => {
  const [offerModalOpen, setOfferModalOpen] = useState(false);

  const isBuyer = currentUserId === buyerId;
  const isSeller = currentUserId === sellerId;
  const showOfferButton = isBuyer || (isSeller && buyerHasLiked);

  if (!showOfferButton) return null;

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        disabled={hasActiveOffer}
        onClick={() => setOfferModalOpen(true)}
        title={hasActiveOffer ? 'An offer is already pending in this conversation' : undefined}
        className="shrink-0"
      >
        Make an Offer
      </Button>

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
    </>
  );
};

export default ActionBar;
