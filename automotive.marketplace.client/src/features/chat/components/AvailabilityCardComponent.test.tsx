import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import AvailabilityCardComponent from "./AvailabilityCardComponent";
import type { AvailabilityCard } from "../types/AvailabilityCard";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("../utils/timezone", () => ({
  getTimezoneOffsetLabel: () => "UTC+2",
}));

vi.mock("./ShareAvailabilityModal", () => ({
  default: ({ open }: { open: boolean }) =>
    open ? <div data-testid="share-back-modal">ShareBackModal</div> : null,
}));

const mockOnPickSlot = vi.fn();
const mockOnShareBack = vi.fn();
const mockOnCancel = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const pendingCard: AvailabilityCard = {
  id: "avail-1",
  status: "Pending",
  expiresAt: "2025-12-31T00:00:00Z",
  initiatorId: "other-user",
  slots: [
    {
      id: "slot-1",
      startTime: "2025-08-15T09:00:00Z",
      endTime: "2025-08-15T12:00:00Z",
    },
    {
      id: "slot-2",
      startTime: "2025-08-16T14:00:00Z",
      endTime: "2025-08-16T17:00:00Z",
    },
  ],
};

const defaultProps = {
  card: pendingCard,
  currentUserId: "current-user",
  onPickSlot: mockOnPickSlot,
  onShareBack: mockOnShareBack,
  onCancel: mockOnCancel,
};

describe("AvailabilityCardComponent", () => {
  it("renders availability card with status label", () => {
    render(<AvailabilityCardComponent {...defaultProps} />);
    expect(
      screen.getByText("availabilityCard.statusLabels.shared"),
    ).toBeInTheDocument();
  });

  it("renders slot information", () => {
    render(<AvailabilityCardComponent {...defaultProps} />);
    // Should render both slots with timezone info
    const timezoneLabels = screen.getAllByText(/UTC\+2/);
    expect(timezoneLabels.length).toBe(2);
  });

  it("shows propose buttons for responder", () => {
    render(<AvailabilityCardComponent {...defaultProps} />);
    const proposeButtons = screen.getAllByText("availabilityCard.propose");
    expect(proposeButtons.length).toBe(2);
  });

  it("shows 'none of these work' link for responder", () => {
    render(<AvailabilityCardComponent {...defaultProps} />);
    expect(
      screen.getByText("availabilityCard.noneOfTheseWork"),
    ).toBeInTheDocument();
  });

  it("shows cancel button for initiator", () => {
    render(
      <AvailabilityCardComponent
        {...defaultProps}
        card={{ ...pendingCard, initiatorId: "current-user" }}
      />,
    );
    expect(
      screen.getByText("availabilityCard.cancelAvailability"),
    ).toBeInTheDocument();
  });

  it("calls onCancel when cancel button clicked", async () => {
    const user = userEvent.setup();
    render(
      <AvailabilityCardComponent
        {...defaultProps}
        card={{ ...pendingCard, initiatorId: "current-user" }}
      />,
    );
    await user.click(
      screen.getByText("availabilityCard.cancelAvailability"),
    );
    expect(mockOnCancel).toHaveBeenCalledWith("avail-1");
  });

  it("hides actions for expired status", () => {
    render(
      <AvailabilityCardComponent
        {...defaultProps}
        card={{ ...pendingCard, status: "Expired" }}
      />,
    );
    expect(
      screen.queryByText("availabilityCard.propose"),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText("availabilityCard.cancelAvailability"),
    ).not.toBeInTheDocument();
  });

  it("opens share-back modal when 'none of these work' clicked", async () => {
    const user = userEvent.setup();
    render(<AvailabilityCardComponent {...defaultProps} />);
    await user.click(screen.getByText("availabilityCard.noneOfTheseWork"));
    expect(screen.getByTestId("share-back-modal")).toBeInTheDocument();
  });
});
