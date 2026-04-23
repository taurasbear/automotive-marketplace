import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { format } from "date-fns";
import { Ban, CalendarRange, Clock } from "lucide-react";
import { useState } from "react";
import type {
  AvailabilityCard,
  AvailabilityCardStatus,
  AvailabilitySlot,
} from "../types/AvailabilityCard";
import { getTimezoneOffsetLabel } from "../utils/timezone";
import ShareAvailabilityModal from "./ShareAvailabilityModal";

const statusConfig: Record<
  AvailabilityCardStatus,
  {
    headerClass: string;
    borderClass: string;
    label: string;
    icon: React.ElementType;
    labelClass: string;
  }
> = {
  Pending: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    label: "Availability Shared",
    icon: CalendarRange,
    labelClass: "text-sky-200",
  },
  Responded: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    label: "Availability Responded",
    icon: CalendarRange,
    labelClass: "text-muted",
  },
  Expired: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    label: "Availability Expired",
    icon: Clock,
    labelClass: "text-muted",
  },
  Cancelled: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    label: "Availability Cancelled",
    icon: Ban,
    labelClass: "text-muted",
  },
};

type AvailabilityCardComponentProps = {
  card: AvailabilityCard;
  currentUserId: string;
  onPickSlot: (
    cardId: string,
    slotId: string,
    startTime: string,
    durationMinutes: number,
  ) => void;
  onShareBack: (
    cardId: string,
    slots: { startTime: string; endTime: string }[],
  ) => void;
  onCancel: (cardId: string) => void;
};

const DURATION_OPTIONS = [15, 30, 60, 90, 120];

const AvailabilityCardComponent = ({
  card,
  currentUserId,
  onPickSlot,
  onShareBack,
  onCancel,
}: AvailabilityCardComponentProps) => {
  const [shareBackOpen, setShareBackOpen] = useState(false);
  const [expandedSlotId, setExpandedSlotId] = useState<string | null>(null);
  const [pickerTime, setPickerTime] = useState("");
  const [pickerDuration, setPickerDuration] = useState(60);
  const [pickerError, setPickerError] = useState<string | null>(null);
  const config = statusConfig[card.status];
  const Icon = config.icon;
  const timezone = getTimezoneOffsetLabel();

  const canRespond =
    card.status === "Pending" && currentUserId !== card.initiatorId;
  const canCancel =
    card.status === "Pending" && currentUserId === card.initiatorId;
  const isDisabled = card.status !== "Pending";

  const handleToggleSlot = (slot: AvailabilitySlot) => {
    if (expandedSlotId === slot.id) {
      setExpandedSlotId(null);
      setPickerError(null);
    } else {
      setExpandedSlotId(slot.id);
      const start = new Date(slot.startTime);
      setPickerTime(
        `${String(start.getHours()).padStart(2, "0")}:${String(start.getMinutes()).padStart(2, "0")}`,
      );
      setPickerDuration(60);
      setPickerError(null);
    }
  };

  const handlePickSlotSubmit = (slot: AvailabilitySlot) => {
    const slotStart = new Date(slot.startTime);
    const slotEnd = new Date(slot.endTime);
    const [hours, minutes] = pickerTime.split(":").map(Number);
    const selectedStart = new Date(slotStart);
    selectedStart.setHours(hours, minutes, 0, 0);
    const selectedEnd = new Date(
      selectedStart.getTime() + pickerDuration * 60000,
    );

    if (selectedStart < slotStart) {
      setPickerError(
        `Start time must be at or after ${format(slotStart, "HH:mm")}`,
      );
      return;
    }
    if (selectedEnd > slotEnd) {
      setPickerError(
        `Meeting must end by ${format(slotEnd, "HH:mm")}`,
      );
      return;
    }

    setPickerError(null);
    setExpandedSlotId(null);
    onPickSlot(card.id, slot.id, selectedStart.toISOString(), pickerDuration);
  };

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
            const isExpanded = expandedSlotId === slot.id;
            return (
              <div key={slot.id}>
                <div
                  className={`flex items-center justify-between px-4 py-2 ${isDisabled ? "opacity-50" : ""}`}
                >
                  <div>
                    <p className="text-sm font-medium">
                      {format(start, "EEE, MMM d")}
                    </p>
                    <p className="text-muted-foreground text-xs">
                      {format(start, "HH:mm")} – {format(end, "HH:mm")}{" "}
                      {timezone}
                    </p>
                  </div>
                  {canRespond && (
                    <Button
                      size="sm"
                      variant="outline"
                      className="h-7 text-xs"
                      onClick={() => handleToggleSlot(slot)}
                    >
                      {isExpanded ? "Close" : "Propose"}
                    </Button>
                  )}
                </div>
                {isExpanded && (
                  <div className="border-t bg-muted/30 px-4 py-2 space-y-2">
                    <div className="flex items-end gap-2">
                      <div className="flex-1 space-y-1">
                        <Label className="text-xs">Start time</Label>
                        <Input
                          type="time"
                          value={pickerTime}
                          onChange={(e) => {
                            setPickerTime(e.target.value);
                            setPickerError(null);
                          }}
                          className="h-7 text-xs"
                        />
                      </div>
                      <div className="flex-1 space-y-1">
                        <Label className="text-xs">Duration</Label>
                        <select
                          value={pickerDuration}
                          onChange={(e) => {
                            setPickerDuration(Number(e.target.value));
                            setPickerError(null);
                          }}
                          className="border-input bg-background h-7 w-full rounded-md border px-2 text-xs"
                        >
                          {DURATION_OPTIONS.map((d) => (
                            <option key={d} value={d}>
                              {d} min
                            </option>
                          ))}
                        </select>
                      </div>
                      <Button
                        size="sm"
                        className="h-7 text-xs"
                        onClick={() => handlePickSlotSubmit(slot)}
                      >
                        Propose
                      </Button>
                    </div>
                    {pickerError && (
                      <p className="text-destructive text-xs">{pickerError}</p>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>

        {canCancel && (
          <div className="border-t px-4 py-2">
            <Button
              size="sm"
              variant="ghost"
              className="text-destructive hover:text-destructive h-7 text-xs"
              onClick={() => onCancel(card.id)}
            >
              Cancel availability
            </Button>
          </div>
        )}

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
