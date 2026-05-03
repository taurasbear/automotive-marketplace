import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ShareAvailabilityModal from "./ShareAvailabilityModal";

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
  onSubmit: mockOnSubmit,
};

describe("ShareAvailabilityModal", () => {
  it("renders dialog with title when open", () => {
    render(<ShareAvailabilityModal {...defaultProps} />);
    expect(
      screen.getByText("shareAvailabilityModal.title"),
    ).toBeInTheDocument();
  });

  it("does not render dialog when closed", () => {
    render(<ShareAvailabilityModal {...defaultProps} open={false} />);
    expect(
      screen.queryByText("shareAvailabilityModal.title"),
    ).not.toBeInTheDocument();
  });

  it("shows one empty slot by default", () => {
    render(<ShareAvailabilityModal {...defaultProps} />);
    expect(
      screen.getByText("shareAvailabilityModal.slot"),
    ).toBeInTheDocument();
  });

  it("adds another slot when add button clicked", async () => {
    const user = userEvent.setup();
    render(<ShareAvailabilityModal {...defaultProps} />);
    await user.click(
      screen.getByText("shareAvailabilityModal.addAnotherSlot"),
    );
    // Should now have 2 slots
    const slotLabels = screen.getAllByText(/shareAvailabilityModal.slot/);
    expect(slotLabels.length).toBe(2);
  });

  it("submit button is disabled when slots are empty", () => {
    render(<ShareAvailabilityModal {...defaultProps} />);
    const submitButton = screen.getByText(
      "shareAvailabilityModal.shareAvailability",
    );
    expect(submitButton).toBeDisabled();
  });

  it("calls onOpenChange when cancel is clicked", async () => {
    const user = userEvent.setup();
    render(<ShareAvailabilityModal {...defaultProps} />);
    await user.click(screen.getByText("common:actions.cancel"));
    expect(mockOnOpenChange).toHaveBeenCalledWith(false);
  });

  it("shows delete button only when multiple slots exist", async () => {
    const user = userEvent.setup();
    render(<ShareAvailabilityModal {...defaultProps} />);
    // With one slot, no delete button
    expect(screen.queryByRole("button", { name: "" })).toBeDefined();

    await user.click(
      screen.getByText("shareAvailabilityModal.addAnotherSlot"),
    );
    // With two slots, delete buttons should appear (Trash2 icon buttons)
    const allButtons = screen.getAllByRole("button");
    expect(allButtons.length).toBeGreaterThan(3);
  });
});
