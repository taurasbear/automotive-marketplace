import { Button } from "@/components/ui/button";
import { BadgeCheck, BadgeX, Clock, HandCoins, Undo2, Ban } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { formatCurrency } from "@/lib/i18n/formatNumber";
import type { Offer } from "../types/Offer";
import MakeOfferModal from "./MakeOfferModal";

type OfferCardProps = {
  offer: Offer;
  currentUserId: string;
  listingPrice: number;
  onAccept: (offerId: string) => void;
  onDecline: (offerId: string) => void;
  onCounter: (offerId: string, amount: number) => void;
  onCancel: (offerId: string) => void;
};

const statusConfig = {
  Pending: {
    headerClass: "bg-slate-900",
    borderClass: "border-border",
    labelKey: "offerCard.offer",
    icon: HandCoins,
    labelClass: "text-slate-200",
    subLabelKey: "offerCard.statusLabels.pending",
    subLabelClass: "text-slate-400",
    priceClass: "text-foreground",
    badgeClass: "bg-red-100 text-red-600 dark:bg-red-950 dark:text-red-400",
  },
  Accepted: {
    headerClass: "bg-green-900",
    borderClass: "border-green-300 dark:border-green-800",
    labelKey: "offerCard.statusLabels.accepted",
    icon: BadgeCheck,
    labelClass: "text-green-200",
    subLabelKey: "offerCard.subtitles.listingOnHold",
    subLabelClass: "text-green-400",
    priceClass: "text-green-600 dark:text-green-400",
    badgeClass:
      "bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-400",
  },
  Declined: {
    headerClass: "bg-red-900",
    borderClass: "border-red-300 dark:border-red-800",
    labelKey: "offerCard.statusLabels.declined",
    icon: BadgeX,
    labelClass: "text-red-200",
    subLabelKey: "offerCard.subtitles.noDealReached",
    subLabelClass: "text-red-400",
    priceClass: "text-muted-foreground line-through",
    badgeClass: "bg-muted text-muted-foreground",
  },
  Countered: {
    headerClass: "bg-violet-900",
    borderClass: "border-violet-300 dark:border-violet-800",
    labelKey: "offerCard.counterOffer",
    icon: Undo2,
    labelClass: "text-violet-200",
    subLabelKey: "offerCard.subtitles.awaitingResponse",
    subLabelClass: "text-violet-400",
    priceClass: "text-foreground",
    badgeClass:
      "bg-violet-100 text-violet-700 dark:bg-violet-950 dark:text-violet-400",
  },
  Expired: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "offerCard.statusLabels.expired",
    icon: Clock,
    labelClass: "text-muted",
    subLabelKey: "offerCard.subtitles.noResponseWithin48h",
    subLabelClass: "text-muted-foreground",
    priceClass: "text-muted-foreground line-through",
    badgeClass: "bg-muted text-muted-foreground",
  },
  Cancelled: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "offerCard.statusLabels.cancelled",
    icon: Ban,
    labelClass: "text-muted",
    subLabelKey: "offerCard.subtitles.cancelled",
    subLabelClass: "text-muted-foreground",
    priceClass: "text-muted-foreground line-through",
    badgeClass: "bg-muted text-muted-foreground",
  },
} as const;

const OfferCard = ({
  offer,
  currentUserId,
  listingPrice,
  onAccept,
  onDecline,
  onCounter,
  onCancel,
}: OfferCardProps) => {
  const [counterModalOpen, setCounterModalOpen] = useState(false);
  const { t } = useTranslation("chat");
  const config = statusConfig[offer.status];
  const Icon = config.icon;

  const canRespond =
    offer.status === "Pending" && currentUserId !== offer.initiatorId;

  const canCancel =
    offer.status === "Pending" && currentUserId === offer.initiatorId;

  return (
    <>
      <div
        className={`w-full max-w-[280px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}
      >
        <div
          className={`${config.headerClass} flex flex-col px-4 py-2.5`}
        >
          <div className="flex items-center gap-2">
            <Icon className={`h-3.5 w-3.5 shrink-0 ${config.labelClass}`} />
            <span
              className={`text-xs font-semibold tracking-wider uppercase ${config.labelClass}`}
            >
              {t(config.labelKey)}
            </span>
          </div>
          <span className={`mt-0.5 text-xs ${config.subLabelClass}`}>
            {t(config.subLabelKey)}
          </span>
        </div>

        <div className="bg-card px-4 py-3">
          <div className="mb-1 flex items-baseline gap-2">
            <span className={`text-xl font-bold ${config.priceClass}`}>
              €{formatCurrency(offer.amount)}
            </span>
            {offer.status !== "Declined" && offer.status !== "Expired" && offer.status !== "Cancelled" && (
              <span className="text-muted-foreground text-xs line-through">
                €{formatCurrency(listingPrice)}
              </span>
            )}
            <span
              className={`rounded-full px-1.5 py-0.5 text-[10px] font-semibold ${config.badgeClass}`}
            >
              −{offer.percentageOff}%
            </span>
          </div>

          {canRespond && (
            <div className="mt-3 flex flex-col gap-2">
              <div className="flex gap-2">
                <Button
                  size="sm"
                  className="h-7 flex-1 text-xs"
                  onClick={() => onAccept(offer.id)}
                >
                  {t("offerCard.actions.accept")}
                </Button>
                <Button
                  size="sm"
                  variant="ghost"
                  className="text-destructive hover:text-destructive h-7 flex-1 text-xs"
                  onClick={() => onDecline(offer.id)}
                >
                  {t("offerCard.actions.decline")}
                </Button>
              </div>
              <Button
                size="sm"
                variant="outline"
                className="h-7 w-full text-xs"
                onClick={() => setCounterModalOpen(true)}
              >
                {t("offerCard.actions.counter")}
              </Button>
            </div>
          )}

          {canCancel && (
            <div className="mt-3 flex gap-2">
              <Button
                size="sm"
                variant="ghost"
                className="text-muted-foreground hover:text-destructive h-7 w-full text-xs"
                onClick={() => onCancel(offer.id)}
              >
                {t("offerCard.actions.cancel")}
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
