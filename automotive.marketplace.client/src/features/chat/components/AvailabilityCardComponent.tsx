import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useDateLocale } from "@/lib/i18n/dateLocale";
import { format } from "date-fns";
import { Ban, CalendarRange, Clock } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
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
    labelKey: string;
    icon: React.ElementType;
    labelClass: string;
  }
> = {
  Pending: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "availabilityCard.statusLabels.shared",
    icon: CalendarRange,
    labelClass: "text-sky-200",
  },
  Responded: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "availabilityCard.statusLabels.responded",
    icon: CalendarRange,
    labelClass: "text-muted",
  },
  Expired: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "availabilityCard.statusLabels.expired",
    icon: Clock,
    labelClass: "text-muted",
  },
  Cancelled: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "availabilityCard.statusLabels.cancelled",
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
  const { t } = useTranslation("chat");
  const locale = useDateLocale();
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
        t("availabilityCard.startTimeMustBeAtOrAfter", {
          time: format(slotStart, "HH:mm", { locale }),
        }),
      );
      return;
    }
    if (selectedEnd > slotEnd) {
      setPickerError(
        t("availabilityCard.meetingMustEndBy", {
          time: format(slotEnd, "HH:mm", { locale }),
        }),
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
            {t(config.labelKey)}
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
                      {format(start, "EEE, MMM d", { locale })}
                    </p>
                    <p className="text-muted-foreground text-xs">
                      {format(start, "HH:mm", { locale })} –{" "}
                      {format(end, "HH:mm", { locale })} {timezone}
                    </p>
                  </div>
                  {canRespond && (
                    <Button
                      size="sm"
                      variant="outline"
                      className="h-7 text-xs"
                      onClick={() => handleToggleSlot(slot)}
                    >
                      {isExpanded
                        ? t("availabilityCard.close")
                        : t("availabilityCard.propose")}
                    </Button>
                  )}
                </div>
                {isExpanded && (
                  <div className="bg-muted/30 space-y-2 border-t px-4 py-2">
                    <div className="flex items-end gap-2">
                      <div className="flex-1 space-y-1">
                        <Label className="text-xs">
                          {t("availabilityCard.startTime")}
                        </Label>
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
                        <Label className="text-xs">
                          {t("availabilityCard.duration")}
                        </Label>
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
                              {t("proposeMeetingModal.durationMinutes", { d })}
                            </option>
                          ))}
                        </select>
                      </div>
                      <Button
                        size="sm"
                        className="h-7 text-xs"
                        onClick={() => handlePickSlotSubmit(slot)}
                      >
                        {t("availabilityCard.propose")}
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
              {t("availabilityCard.cancelAvailability")}
            </Button>
          </div>
        )}

        {canRespond && (
          <div className="border-t px-4 py-2">
            <button
              className="text-muted-foreground hover:text-foreground text-xs underline"
              onClick={() => setShareBackOpen(true)}
            >
              {t("availabilityCard.noneOfTheseWork")}
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
