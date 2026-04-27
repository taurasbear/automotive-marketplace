import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Ban, CheckCircle, Clock, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";
import { format } from "date-fns";
import { useDateLocale } from "@/lib/i18n/dateLocale";
import type { ContractCard, ContractCardStatus } from "../types/ContractCard";

type ContractCardProps = {
  card: ContractCard;
  currentUserId: string;
  isSeller: boolean;
  onAccept: (cardId: string) => void;
  onDecline: (cardId: string) => void;
  onCancel: (cardId: string) => void;
  onFillOut: (cardId: string) => void;
  onViewSubmitted: (cardId: string) => void;
  onExportPdf: (cardId: string) => void;
};

type StatusConfig = {
  headerClass: string;
  borderClass: string;
  labelKey: string;
  icon: React.ElementType;
  labelClass: string;
};

const statusConfig: Record<ContractCardStatus, StatusConfig> = {
  Pending: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.pending",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  Active: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.active",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  SellerSubmitted: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.active",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  BuyerSubmitted: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.active",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  Complete: {
    headerClass: "bg-green-900",
    borderClass: "border-green-300 dark:border-green-800",
    labelKey: "contractCard.statusLabels.complete",
    icon: CheckCircle,
    labelClass: "text-green-200",
  },
  Declined: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "contractCard.statusLabels.declined",
    icon: Ban,
    labelClass: "text-muted",
  },
  Cancelled: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "contractCard.statusLabels.cancelled",
    icon: Ban,
    labelClass: "text-muted",
  },
};

const ContractCardComponent = ({
  card,
  currentUserId,
  isSeller,
  onAccept,
  onDecline,
  onCancel,
  onFillOut,
  onViewSubmitted,
  onExportPdf,
}: ContractCardProps) => {
  const { t } = useTranslation("chat");
  const locale = useDateLocale();
  const config = statusConfig[card.status];
  const Icon = config.icon;

  const isInitiator = card.initiatorId === currentUserId;
  const isRecipient = !isInitiator;

  const sellerSubmitted = !!card.sellerSubmittedAt;
  const buyerSubmitted = !!card.buyerSubmittedAt;

  const callerHasSubmitted = isSeller ? sellerSubmitted : buyerSubmitted;
  const canFillOut =
    (card.status === "Active" ||
      (card.status === "SellerSubmitted" && !isSeller) ||
      (card.status === "BuyerSubmitted" && isSeller)) &&
    !callerHasSubmitted;

  return (
    <div
      className={`w-full max-w-[300px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}
    >
      {/* Header */}
      <div className={`${config.headerClass} flex items-center gap-2 px-4 py-2.5`}>
        <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
        <span className={`text-xs font-semibold tracking-wider uppercase ${config.labelClass}`}>
          {t(config.labelKey)}
        </span>
      </div>

      {/* Body */}
      <div className="bg-card px-4 py-3 space-y-3">
        {/* Pending — recipient sees accept/decline */}
        {card.status === "Pending" && isRecipient && (
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{t("contractCard.recipientPendingMessage")}</p>
            <div className="flex gap-2">
              <Button size="sm" className="flex-1" onClick={() => onAccept(card.id)}>
                {t("contractCard.accept")}
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="flex-1"
                onClick={() => onDecline(card.id)}
              >
                {t("contractCard.decline")}
              </Button>
            </div>
          </div>
        )}

        {/* Pending — initiator waits */}
        {card.status === "Pending" && isInitiator && (
          <div className="space-y-2">
            <div className="flex items-center gap-2 text-muted-foreground">
              <Clock className="h-3.5 w-3.5" />
              <span className="text-xs">{t("contractCard.waitingForResponse")}</span>
            </div>
            <Button
              size="sm"
              variant="ghost"
              className="text-destructive hover:text-destructive h-7 text-xs w-full"
              onClick={() => onCancel(card.id)}
            >
              {t("contractCard.cancel")}
            </Button>
          </div>
        )}

        {/* Active / SellerSubmitted / BuyerSubmitted — show badges */}
        {["Active", "SellerSubmitted", "BuyerSubmitted"].includes(card.status) && (
          <div className="space-y-2">
            <div className="flex flex-wrap gap-2">
              <Badge
                variant={sellerSubmitted ? "default" : "secondary"}
                className={sellerSubmitted ? "bg-green-600 text-white" : "bg-yellow-600/20 text-yellow-700 dark:text-yellow-400"}
              >
                {t("contractCard.seller")}: {sellerSubmitted ? t("contractCard.submitted") : t("contractCard.pending")}
              </Badge>
              <Badge
                variant={buyerSubmitted ? "default" : "secondary"}
                className={buyerSubmitted ? "bg-green-600 text-white" : "bg-yellow-600/20 text-yellow-700 dark:text-yellow-400"}
              >
                {t("contractCard.buyer")}: {buyerSubmitted ? t("contractCard.submitted") : t("contractCard.pending")}
              </Badge>
            </div>
            {canFillOut && (
              <Button size="sm" className="w-full" onClick={() => onFillOut(card.id)}>
                {t("contractCard.fillOut")}
              </Button>
            )}
            {callerHasSubmitted && (
              <Button size="sm" variant="outline" className="w-full" onClick={() => onViewSubmitted(card.id)}>
                {t("contractCard.viewSubmittedData")} ·{" "}
                {isSeller
                  ? format(new Date(card.sellerSubmittedAt!), "MMM d", { locale })
                  : format(new Date(card.buyerSubmittedAt!), "MMM d", { locale })}
              </Button>
            )}
          </div>
        )}

        {/* Complete */}
        {card.status === "Complete" && (
          <div className="space-y-2">
            <div className="flex gap-2">
              <Badge className="bg-green-600 text-white">
                {t("contractCard.seller")}: {t("contractCard.submitted")}
              </Badge>
              <Badge className="bg-green-600 text-white">
                {t("contractCard.buyer")}: {t("contractCard.submitted")}
              </Badge>
            </div>
            <Button size="sm" className="w-full" onClick={() => onExportPdf(card.id)}>
              <FileText className="mr-2 h-4 w-4" />
              {t("contractCard.exportPdf")}
            </Button>
          </div>
        )}

        {/* Declined / Cancelled */}
        {(card.status === "Declined" || card.status === "Cancelled") && (
          <p className="text-xs text-muted-foreground">
            {card.status === "Declined"
              ? t("contractCard.declinedMessage")
              : t("contractCard.cancelledMessage")}
          </p>
        )}
      </div>
    </div>
  );
};

export default ContractCardComponent;
