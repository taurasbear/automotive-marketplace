import { Button } from '@/components/ui/button';
import { BadgeCheck, BadgeX, Clock, HandCoins, Undo2 } from 'lucide-react';
import { useState } from 'react';
import type { Offer } from '../types/Offer';
import MakeOfferModal from './MakeOfferModal';

type OfferCardProps = {
  offer: Offer;
  currentUserId: string;
  listingPrice: number;
  onAccept: (offerId: string) => void;
  onDecline: (offerId: string) => void;
  onCounter: (offerId: string, amount: number) => void;
};

const statusConfig = {
  Pending: {
    headerClass: 'bg-slate-900',
    borderClass: 'border-border',
    label: 'Offer',
    icon: HandCoins,
    labelClass: 'text-slate-200',
    subLabel: 'Pending response',
    subLabelClass: 'text-slate-400',
    priceClass: 'text-foreground',
    badgeClass: 'bg-red-100 text-red-600 dark:bg-red-950 dark:text-red-400',
  },
  Accepted: {
    headerClass: 'bg-green-900',
    borderClass: 'border-green-300 dark:border-green-800',
    label: 'Offer Accepted',
    icon: BadgeCheck,
    labelClass: 'text-green-200',
    subLabel: 'Listing is now on hold',
    subLabelClass: 'text-green-400',
    priceClass: 'text-green-600 dark:text-green-400',
    badgeClass: 'bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-400',
  },
  Declined: {
    headerClass: 'bg-red-900',
    borderClass: 'border-red-300 dark:border-red-800',
    label: 'Offer Declined',
    icon: BadgeX,
    labelClass: 'text-red-200',
    subLabel: 'No deal reached',
    subLabelClass: 'text-red-400',
    priceClass: 'text-muted-foreground line-through',
    badgeClass: 'bg-muted text-muted-foreground',
  },
  Countered: {
    headerClass: 'bg-violet-900',
    borderClass: 'border-violet-300 dark:border-violet-800',
    label: 'Counter-Offer',
    icon: Undo2,
    labelClass: 'text-violet-200',
    subLabel: 'Awaiting response',
    subLabelClass: 'text-violet-400',
    priceClass: 'text-foreground',
    badgeClass: 'bg-violet-100 text-violet-700 dark:bg-violet-950 dark:text-violet-400',
  },
  Expired: {
    headerClass: 'bg-muted-foreground/60',
    borderClass: 'border-border',
    label: 'Offer Expired',
    icon: Clock,
    labelClass: 'text-muted',
    subLabel: 'No response within 48 hours',
    subLabelClass: 'text-muted-foreground',
    priceClass: 'text-muted-foreground line-through',
    badgeClass: 'bg-muted text-muted-foreground',
  },
} as const;

const OfferCard = ({
  offer,
  currentUserId,
  listingPrice,
  onAccept,
  onDecline,
  onCounter,
}: OfferCardProps) => {
  const [counterModalOpen, setCounterModalOpen] = useState(false);
  const config = statusConfig[offer.status];
  const Icon = config.icon;

  const canRespond = offer.status === 'Pending' && currentUserId !== offer.initiatorId;

  return (
    <>
      <div className={`w-full max-w-[280px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}>
        <div className={`${config.headerClass} flex items-center justify-between px-4 py-2.5`}>
          <div className="flex items-center gap-2">
            <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
            <span className={`text-xs font-semibold uppercase tracking-wider ${config.labelClass}`}>
              {config.label}
            </span>
          </div>
          <span className={`text-xs ${config.subLabelClass}`}>{config.subLabel}</span>
        </div>

        <div className="bg-card px-4 py-3">
          <div className="mb-1 flex items-baseline gap-2">
            <span className={`text-xl font-bold ${config.priceClass}`}>
              €{offer.amount.toLocaleString()}
            </span>
            {offer.status !== 'Declined' && offer.status !== 'Expired' && (
              <span className="text-muted-foreground text-xs line-through">
                €{listingPrice.toLocaleString()}
              </span>
            )}
            <span className={`rounded-full px-1.5 py-0.5 text-[10px] font-semibold ${config.badgeClass}`}>
              −{offer.percentageOff}%
            </span>
          </div>

          {canRespond && (
            <div className="mt-3 flex gap-2">
              <Button
                size="sm"
                className="h-7 flex-1 text-xs"
                onClick={() => onAccept(offer.id)}
              >
                Accept
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="h-7 flex-1 text-xs"
                onClick={() => setCounterModalOpen(true)}
              >
                Counter
              </Button>
              <Button
                size="sm"
                variant="ghost"
                className="text-destructive hover:text-destructive h-7 flex-1 text-xs"
                onClick={() => onDecline(offer.id)}
              >
                Decline
              </Button>
            </div>
          )}
        </div>
      </div>

      <MakeOfferModal
        open={counterModalOpen}
        onOpenChange={setCounterModalOpen}
        mode="counter"
        listingPrice={listingPrice}
        initialAmount={offer.amount}
        offerId={offer.id}
        onSubmit={(amount) => {
          onCounter(offer.id, amount);
          setCounterModalOpen(false);
        }}
      />
    </>
  );
};

export default OfferCard;
