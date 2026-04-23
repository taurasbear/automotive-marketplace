import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { MapPin } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { Meeting } from "../types/Meeting";
import { getTimezoneOffsetLabel } from "../utils/timezone";

type ProposeMeetingModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: "propose" | "reschedule";
  initialMeeting?: Meeting;
  onSubmit: (data: {
    proposedAt: string;
    durationMinutes: number;
    locationText?: string;
    locationLat?: number;
    locationLng?: number;
  }) => void;
};

const DURATION_PRESETS = [30, 60, 90, 120];

const ProposeMeetingModal = ({
  open,
  onOpenChange,
  mode,
  initialMeeting,
  onSubmit,
}: ProposeMeetingModalProps) => {
  const { t } = useTranslation(["chat", "common"]);
  const timezone = getTimezoneOffsetLabel();
  const now = new Date();

  const formatLocalDate = (d: Date) =>
    `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
  const formatLocalTime = (d: Date) =>
    `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;

  const defaultDate = initialMeeting
    ? formatLocalDate(new Date(initialMeeting.proposedAt))
    : "";
  const defaultTime = initialMeeting
    ? formatLocalTime(new Date(initialMeeting.proposedAt))
    : "";

  const [date, setDate] = useState(defaultDate);
  const [time, setTime] = useState(defaultTime);
  const [duration, setDuration] = useState(
    initialMeeting?.durationMinutes ?? 60,
  );
  const [locationText, setLocationText] = useState(
    initialMeeting?.locationText ?? "",
  );
  const [showCoords, setShowCoords] = useState(false);
  const [lat, setLat] = useState(initialMeeting?.locationLat?.toString() ?? "");
  const [lng, setLng] = useState(initialMeeting?.locationLng?.toString() ?? "");

  const proposedAt = date && time ? new Date(`${date}T${time}:00`) : null;
  const isInFuture = proposedAt ? proposedAt > now : false;
  const isValid = !!date && !!time && isInFuture && duration > 0;

  const handleSubmit = () => {
    if (!isValid || !proposedAt) return;
    onSubmit({
      proposedAt: proposedAt.toISOString(),
      durationMinutes: duration,
      locationText: locationText || undefined,
      locationLat: lat ? parseFloat(lat) : undefined,
      locationLng: lng ? parseFloat(lng) : undefined,
    });
    onOpenChange(false);
  };

  const handleUseMyLocation = () => {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setLat(pos.coords.latitude.toFixed(7));
        setLng(pos.coords.longitude.toFixed(7));
        setShowCoords(true);
      },
      () => setShowCoords(true),
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>
            {mode === "reschedule" ? t("proposeMeetingModal.rescheduleTitle") : t("proposeMeetingModal.proposeTitle")}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="space-y-1.5">
            <Label htmlFor="meeting-date">{t("proposeMeetingModal.date")}</Label>
            <Input
              id="meeting-date"
              type="date"
              value={date}
              min={now.toISOString().slice(0, 10)}
              onChange={(e) => setDate(e.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="meeting-time">{t("proposeMeetingModal.startTime", { timezone })}</Label>
            <Input
              id="meeting-time"
              type="time"
              value={time}
              onChange={(e) => setTime(e.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label>{t("proposeMeetingModal.duration")}</Label>
            <div className="flex gap-2">
              {DURATION_PRESETS.map((d) => (
                <Button
                  key={d}
                  type="button"
                  size="sm"
                  variant={duration === d ? "default" : "outline"}
                  className="h-7 text-xs"
                  onClick={() => setDuration(d)}
                >
                  {t("proposeMeetingModal.durationMinutes", { d })}
                </Button>
              ))}
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="meeting-location">{t("proposeMeetingModal.locationOptional")}</Label>
            <Input
              id="meeting-location"
              placeholder={t("proposeMeetingModal.locationPlaceholder")}
              value={locationText}
              onChange={(e) => setLocationText(e.target.value)}
            />
          </div>

          {!showCoords && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="text-xs"
              onClick={handleUseMyLocation}
            >
              <MapPin className="mr-1 h-3.5 w-3.5" />
              {t("proposeMeetingModal.setPinOptional")}
            </Button>
          )}

          {showCoords && (
            <div className="grid grid-cols-2 gap-2">
              <div className="space-y-1">
                <Label htmlFor="lat" className="text-xs">
                  {t("proposeMeetingModal.latitude")}
                </Label>
                <Input
                  id="lat"
                  type="number"
                  step="any"
                  value={lat}
                  onChange={(e) => setLat(e.target.value)}
                  placeholder={t("proposeMeetingModal.latitudePlaceholder")}
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="lng" className="text-xs">
                  {t("proposeMeetingModal.longitude")}
                </Label>
                <Input
                  id="lng"
                  type="number"
                  step="any"
                  value={lng}
                  onChange={(e) => setLng(e.target.value)}
                  placeholder={t("proposeMeetingModal.longitudePlaceholder")}
                />
              </div>
            </div>
          )}

          {date && time && !isInFuture && (
            <p className="text-destructive text-xs">
              {t("proposeMeetingModal.mustBeInFuture")}
            </p>
          )}

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              {t("common:actions.cancel")}
            </Button>
            <Button onClick={handleSubmit} disabled={!isValid}>
              {mode === "reschedule" ? t("proposeMeetingModal.sendReschedule") : t("proposeMeetingModal.proposeMeetup")}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default ProposeMeetingModal;
