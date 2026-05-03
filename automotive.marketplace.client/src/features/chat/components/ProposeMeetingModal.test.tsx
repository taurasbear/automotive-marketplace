import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ProposeMeetingModal from "./ProposeMeetingModal";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("../utils/timezone", () => ({
  getTimezoneOffsetLabel: () => "UTC+2",
}));

const mockOnSubmit = vi.fn();
const mockOnOpenChange = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const defaultProps = {
  open: true,
  onOpenChange: mockOnOpenChange,
  mode: "propose" as const,
  onSubmit: mockOnSubmit,
};

describe("ProposeMeetingModal", () => {
  it("renders dialog with propose title when open", () => {
    render(<ProposeMeetingModal {...defaultProps} />);
    expect(
      screen.getByText("proposeMeetingModal.proposeTitle"),
    ).toBeInTheDocument();
  });

  it("renders dialog with reschedule title when mode is reschedule", () => {
    render(<ProposeMeetingModal {...defaultProps} mode="reschedule" />);
    expect(
      screen.getByText("proposeMeetingModal.rescheduleTitle"),
    ).toBeInTheDocument();
  });

  it("does not render dialog when closed", () => {
    render(<ProposeMeetingModal {...defaultProps} open={false} />);
    expect(
      screen.queryByText("proposeMeetingModal.proposeTitle"),
    ).not.toBeInTheDocument();
  });

  it("shows date and time inputs", () => {
    render(<ProposeMeetingModal {...defaultProps} />);
    expect(
      screen.getByLabelText("proposeMeetingModal.date"),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText(/proposeMeetingModal.startTime/),
    ).toBeInTheDocument();
  });

  it("shows duration presets", () => {
    render(<ProposeMeetingModal {...defaultProps} />);
    const buttons = screen.getAllByRole("button");
    // Duration presets: 30, 60, 90, 120 + cancel + submit = 6 buttons
    expect(buttons.length).toBeGreaterThanOrEqual(4);
  });

  it("submit button is disabled when no date/time entered", () => {
    render(<ProposeMeetingModal {...defaultProps} />);
    const submitButton = screen.getByText("proposeMeetingModal.proposeMeetup");
    expect(submitButton).toBeDisabled();
  });

  it("calls onOpenChange when cancel is clicked", async () => {
    const user = userEvent.setup();
    render(<ProposeMeetingModal {...defaultProps} />);
    await user.click(screen.getByText("common:actions.cancel"));
    expect(mockOnOpenChange).toHaveBeenCalledWith(false);
  });
});
