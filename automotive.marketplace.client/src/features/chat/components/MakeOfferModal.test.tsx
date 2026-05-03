import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import MakeOfferModal from "./MakeOfferModal";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (val: number) => String(val),
}));

const mockOnSubmit = vi.fn();
const mockOnOpenChange = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const defaultProps = {
  open: true,
  onOpenChange: mockOnOpenChange,
  mode: "offer" as const,
  listingPrice: 30000,
  onSubmit: mockOnSubmit,
};

describe("MakeOfferModal", () => {
  it("renders dialog with offer title when open", () => {
    render(<MakeOfferModal {...defaultProps} />);
    expect(
      screen.getByText("makeOfferModal.makeOfferTitle"),
    ).toBeInTheDocument();
  });

  it("renders dialog with counter title when mode is counter", () => {
    render(<MakeOfferModal {...defaultProps} mode="counter" />);
    expect(
      screen.getByText("makeOfferModal.counterOfferTitle"),
    ).toBeInTheDocument();
  });

  it("does not render dialog when closed", () => {
    render(<MakeOfferModal {...defaultProps} open={false} />);
    expect(
      screen.queryByText("makeOfferModal.makeOfferTitle"),
    ).not.toBeInTheDocument();
  });

  it("shows listing price", () => {
    render(<MakeOfferModal {...defaultProps} />);
    expect(screen.getByText("€30000")).toBeInTheDocument();
  });

  it("shows price input field", () => {
    render(<MakeOfferModal {...defaultProps} />);
    expect(
      screen.getByLabelText("makeOfferModal.yourOffer"),
    ).toBeInTheDocument();
  });

  it("submit button is disabled when no amount entered", () => {
    render(<MakeOfferModal {...defaultProps} />);
    const submitButton = screen.getByText("makeOfferModal.sendOffer");
    expect(submitButton).toBeDisabled();
  });

  it("calls onSubmit with valid amount", async () => {
    const user = userEvent.setup();
    render(<MakeOfferModal {...defaultProps} />);

    const input = screen.getByLabelText("makeOfferModal.yourOffer");
    await user.type(input, "20000");
    await user.click(screen.getByText("makeOfferModal.sendOffer"));

    expect(mockOnSubmit).toHaveBeenCalledWith(20000);
  });

  it("shows error when amount is too low", async () => {
    const user = userEvent.setup();
    render(<MakeOfferModal {...defaultProps} />);

    const input = screen.getByLabelText("makeOfferModal.yourOffer");
    await user.type(input, "5000");

    expect(screen.getByText(/makeOfferModal.minOffer/)).toBeInTheDocument();
  });

  it("shows error when amount is too high", async () => {
    const user = userEvent.setup();
    render(<MakeOfferModal {...defaultProps} />);

    const input = screen.getByLabelText("makeOfferModal.yourOffer");
    await user.type(input, "35000");

    expect(screen.getByText(/makeOfferModal.maxOffer/)).toBeInTheDocument();
  });

  it("calls onOpenChange when cancel is clicked", async () => {
    const user = userEvent.setup();
    render(<MakeOfferModal {...defaultProps} />);
    await user.click(screen.getByText("common:actions.cancel"));
    expect(mockOnOpenChange).toHaveBeenCalledWith(false);
  });
});
