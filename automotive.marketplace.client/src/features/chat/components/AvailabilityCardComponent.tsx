import { Button } from "@/components/ui/button";
import { format } from "date-fns";
import { CalendarRange, Clock } from "lucide-react";
import { useState } from "react";
import type {
  AvailabilityCard,
  AvailabilityCardStatus,
} from "../types/AvailabilityCard";
import ShareAvailabilityModal from "./ShareAvailabilityModal";

type AvailabilityCardComponentProps = {
  card: AvailabilityCard;
  currentUserId: string;
  onPickSlot: (cardId: string, slotId: string) => void;
  onShareBack: (
    cardId: string,
    slots: { startTime: string; endTime: string }[],
  ) => void;
};

const statusConfig: Record<
  AvailabilityCardStatus,
  {
    headerClass: string;
    borderClass: string;
    label: string;
    icon: typeof CalendarRange;
    labelClass: string;
  }
> = {
  Pending: {
    headerClass: "bg-purple-900",
    borderClass: "border-purple-300 dark:border-purple-800",
    label: "Availability Shared",
    icon: CalendarRange,
    labelClass: "text-purple-200",
  },
  Responded: {
    headerClass: "bg-purple-900/60",
    borderClass: "border-border",
    label: "Responded",
    icon: CalendarRange,
    labelClass: "text-purple-300",
  },
  Expired: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    label: "Availability Expired",
    icon: Clock,
    labelClass: "text-muted",
  },
  Cancelled: {
    headerClass: "bg-red-900/60",
    borderClass: "border-red-300 dark:border-red-800",
    label: "Availability Cancelled",
    icon: CalendarRange,
    labelClass: "text-red-300",
  },
};

const AvailabilityCardComponent = ({
  card,
  currentUserId,
  onPickSlot,
  onShareBack,
}: AvailabilityCardComponentProps) => {
  const [shareBackOpen, setShareBackOpen] = useState(false);
  const config = statusConfig[card.status];
  const Icon = config.icon;

  const canRespond =
    card.status === "Pending" && currentUserId !== card.initiatorId;
  const isDisabled = card.status !== "Pending";

  return (
    <>
      <div
        className={`w-full max-w-[300px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}
      >
        <div
          className={`${config.headerClass} flex items-center gap-2 px-4 py-2.5`}
        >
          <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
          <span
            className={`text-xs font-semibold tracking-wider uppercase ${config.labelClass}`}
          >
            {config.label}
          </span>
        </div>

        <div className="bg-card divide-y">
          {card.slots.map((slot) => {
            const start = new Date(slot.startTime);
            const end = new Date(slot.endTime);
            return (
              <div
                key={slot.id}
                className={`flex items-center justify-between px-4 py-2 ${isDisabled ? "opacity-50" : ""}`}
              >
                <div>
                  <p className="text-sm font-medium">
                    {format(start, "EEE, MMM d")}
                  </p>
                  <p className="text-muted-foreground text-xs">
                    {format(start, "HH:mm")} – {format(end, "HH:mm")} UTC
                  </p>
                </div>
                {canRespond && (
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-7 text-xs"
                    onClick={() => onPickSlot(card.id, slot.id)}
                  >
                    Propose →
                  </Button>
                )}
              </div>
            );
          })}
        </div>

        {canRespond && (
          <div className="border-t px-4 py-2">
            <button
              className="text-muted-foreground hover:text-foreground text-xs underline"
              onClick={() => setShareBackOpen(true)}
            >
              None of these work — share my availability instead
            </button>
          </div>
        )}
      </div>

      <ShareAvailabilityModal
        open={shareBackOpen}
        onOpenChange={setShareBackOpen}
        onSubmit={(slots) => {
          onShareBack(card.id, slots);
          setShareBackOpen(false);
        }}
      />
    </>
  );
};

export default AvailabilityCardComponent;
