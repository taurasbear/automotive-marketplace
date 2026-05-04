import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import MeetingCard from "./MeetingCard";
import type { Meeting } from "../types/Meeting";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("../utils/timezone", () => ({
  getTimezoneOffsetLabel: () => "UTC+2",
}));

vi.mock("./ProposeMeetingModal", () => ({
  default: ({ open }: { open: boolean }) =>
    open ? <div data-testid="reschedule-modal">RescheduleModal</div> : null,
}));

vi.mock("./ShareAvailabilityModal", () => ({
  default: ({ open }: { open: boolean }) =>
    open ? <div data-testid="share-avail-modal">ShareAvailModal</div> : null,
}));

const mockOnAccept = vi.fn();
const mockOnDecline = vi.fn();
const mockOnReschedule = vi.fn();
const mockOnCancel = vi.fn();
const mockOnShareAvailability = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const pendingMeeting: Meeting = {
  id: "meeting-1",
  proposedAt: "2025-08-15T14:00:00Z",
  durationMinutes: 60,
  status: "Pending",
  expiresAt: "2025-08-14T14:00:00Z",
  initiatorId: "other-user",
};

const defaultProps = {
  meeting: pendingMeeting,
  currentUserId: "current-user",
  onAccept: mockOnAccept,
  onDecline: mockOnDecline,
  onReschedule: mockOnReschedule,
  onCancel: mockOnCancel,
  onShareAvailability: mockOnShareAvailability,
};

describe("MeetingCard", () => {
  it("renders meeting date and time info", () => {
    render(<MeetingCard {...defaultProps} />);
    expect(screen.getByText(/UTC\+2/)).toBeInTheDocument();
  });

  it("shows accept and decline buttons when user can respond", () => {
    render(<MeetingCard {...defaultProps} />);
    expect(screen.getByText("meetingCard.actions.accept")).toBeInTheDocument();
    expect(screen.getByText("meetingCard.actions.decline")).toBeInTheDocument();
  });

  it("calls onAccept when accept button clicked", async () => {
    const user = userEvent.setup();
    render(<MeetingCard {...defaultProps} />);
    await user.click(screen.getByText("meetingCard.actions.accept"));
    expect(mockOnAccept).toHaveBeenCalledWith("meeting-1");
  });

  it("calls onDecline when decline button clicked", async () => {
    const user = userEvent.setup();
    render(<MeetingCard {...defaultProps} />);
    await user.click(screen.getByText("meetingCard.actions.decline"));
    expect(mockOnDecline).toHaveBeenCalledWith("meeting-1");
  });

  it("shows cancel button when user is initiator", () => {
    render(
      <MeetingCard
        {...defaultProps}
        meeting={{ ...pendingMeeting, initiatorId: "current-user" }}
      />,
    );
    expect(
      screen.getByText("meetingCard.actions.cancelMeetup"),
    ).toBeInTheDocument();
    expect(
      screen.queryByText("meetingCard.actions.accept"),
    ).not.toBeInTheDocument();
  });

  it("calls onCancel when cancel button clicked", async () => {
    const user = userEvent.setup();
    render(
      <MeetingCard
        {...defaultProps}
        meeting={{ ...pendingMeeting, initiatorId: "current-user" }}
      />,
    );
    await user.click(screen.getByText("meetingCard.actions.cancelMeetup"));
    expect(mockOnCancel).toHaveBeenCalledWith("meeting-1");
  });

  it("hides action buttons for accepted meetings but shows cancel", () => {
    render(
      <MeetingCard
        {...defaultProps}
        meeting={{ ...pendingMeeting, status: "Accepted", initiatorId: "current-user" }}
      />,
    );
    expect(
      screen.queryByText("meetingCard.actions.accept"),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText("meetingCard.actions.cancelMeetup"),
    ).toBeInTheDocument();
  });

  it("hides all action buttons for declined meetings", () => {
    render(
      <MeetingCard
        {...defaultProps}
        meeting={{ ...pendingMeeting, status: "Declined" }}
      />,
    );
    expect(
      screen.queryByText("meetingCard.actions.accept"),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText("meetingCard.actions.cancelMeetup"),
    ).not.toBeInTheDocument();
  });
});
