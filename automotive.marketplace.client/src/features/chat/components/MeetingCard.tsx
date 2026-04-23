import { Button } from "@/components/ui/button";
import { format } from "date-fns";
import {
  CalendarCheck,
  CalendarX,
  Clock,
  CalendarDays,
  CalendarClock,
} from "lucide-react";
import { useState } from "react";
import type { Meeting } from "../types/Meeting";
import ProposeMeetingModal from "./ProposeMeetingModal";

type MeetingCardProps = {
  meeting: Meeting;
  currentUserId: string;
  onAccept: (meetingId: string) => void;
  onDecline: (meetingId: string) => void;
  onReschedule: (
    meetingId: string,
    data: {
      proposedAt: string;
      durationMinutes: number;
      locationText?: string;
      locationLat?: number;
      locationLng?: number;
    },
  ) => void;
};

const statusConfig = {
  Pending: {
    headerClass: "bg-[#1e3a5f]",
    borderClass: "border-blue-300 dark:border-blue-800",
    label: "Meetup Proposed",
    icon: CalendarDays,
    labelClass: "text-blue-200",
    subLabel: "Awaiting response",
    subLabelClass: "text-blue-400",
  },
  Accepted: {
    headerClass: "bg-green-900",
    borderClass: "border-green-300 dark:border-green-800",
    label: "Meetup Confirmed",
    icon: CalendarCheck,
    labelClass: "text-green-200",
    subLabel: "See you there!",
    subLabelClass: "text-green-400",
  },
  Declined: {
    headerClass: "bg-red-900",
    borderClass: "border-red-300 dark:border-red-800",
    label: "Meetup Declined",
    icon: CalendarX,
    labelClass: "text-red-200",
    subLabel: "Not happening",
    subLabelClass: "text-red-400",
  },
  Rescheduled: {
    headerClass: "bg-violet-900",
    borderClass: "border-violet-300 dark:border-violet-800",
    label: "Reschedule Proposed",
    icon: CalendarClock,
    labelClass: "text-violet-200",
    subLabel: "Superseded",
    subLabelClass: "text-violet-400",
  },
  Expired: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    label: "Meetup Expired",
    icon: Clock,
    labelClass: "text-muted",
    subLabel: "No response in time",
    subLabelClass: "text-muted-foreground",
  },
} as const;

const MeetingCard = ({
  meeting,
  currentUserId,
  onAccept,
  onDecline,
  onReschedule,
}: MeetingCardProps) => {
  const [rescheduleOpen, setRescheduleOpen] = useState(false);
  const config = statusConfig[meeting.status];
  const Icon = config.icon;

  const canRespond =
    meeting.status === "Pending" && currentUserId !== meeting.initiatorId;
  const proposedDate = new Date(meeting.proposedAt);
  const isMuted =
    meeting.status === "Declined" ||
    meeting.status === "Expired" ||
    meeting.status === "Rescheduled";

  return (
    <>
      <div
        className={`w-full max-w-[280px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}
      >
        <div
          className={`${config.headerClass} flex items-center justify-between px-4 py-2.5`}
        >
          <div className="flex items-center gap-2">
            <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
            <span
              className={`text-xs font-semibold tracking-wider uppercase ${config.labelClass}`}
            >
              {config.label}
            </span>
          </div>
          <span className={`text-xs ${config.subLabelClass}`}>
            {config.subLabel}
          </span>
        </div>

        <div className="bg-card px-4 py-3">
          <div className="mb-2 flex items-center gap-3">
            <div
              className={`flex h-12 w-12 flex-col items-center justify-center rounded-lg ${isMuted ? "bg-muted" : "bg-primary/10"}`}
            >
              <span
                className={`text-lg leading-none font-bold ${isMuted ? "text-muted-foreground" : "text-primary"}`}
              >
                {format(proposedDate, "d")}
              </span>
              <span
                className={`text-[10px] uppercase ${isMuted ? "text-muted-foreground" : "text-primary"}`}
              >
                {format(proposedDate, "MMM")}
              </span>
            </div>
            <div>
              <p
                className={`text-sm font-semibold ${isMuted ? "text-muted-foreground line-through" : ""}`}
              >
                {format(proposedDate, "EEEE")}
              </p>
              <p
                className={`text-xs ${isMuted ? "text-muted-foreground" : "text-muted-foreground"}`}
              >
                {format(proposedDate, "HH:mm")} –{" "}
                {format(
                  new Date(
                    proposedDate.getTime() + meeting.durationMinutes * 60000,
                  ),
                  "HH:mm",
                )}{" "}
                UTC
              </p>
              <p className="text-muted-foreground text-[10px]">
                {meeting.durationMinutes} min
              </p>
            </div>
          </div>

          {meeting.locationText && (
            <p className={`text-xs ${isMuted ? "text-muted-foreground" : ""}`}>
              📍 {meeting.locationText}
            </p>
          )}

          {canRespond && (
            <div className="mt-3 flex gap-2">
              <Button
                size="sm"
                className="h-7 flex-1 text-xs"
                onClick={() => onAccept(meeting.id)}
              >
                Accept
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="h-7 flex-1 text-xs"
                onClick={() => setRescheduleOpen(true)}
              >
                Reschedule
              </Button>
              <Button
                size="sm"
                variant="ghost"
                className="text-destructive hover:text-destructive h-7 flex-1 text-xs"
                onClick={() => onDecline(meeting.id)}
              >
                Decline
              </Button>
            </div>
          )}
        </div>
      </div>

      <ProposeMeetingModal
        open={rescheduleOpen}
        onOpenChange={setRescheduleOpen}
        mode="reschedule"
        initialMeeting={meeting}
        onSubmit={(data) => {
          onReschedule(meeting.id, data);
          setRescheduleOpen(false);
        }}
      />
    </>
  );
};

export default MeetingCard;
