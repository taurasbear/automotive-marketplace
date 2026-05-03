import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import OfferCard from "./OfferCard";
import type { Offer } from "../types/Offer";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (val: number) => String(val),
}));

vi.mock("./MakeOfferModal", () => ({
  default: ({ open }: { open: boolean }) =>
    open ? <div data-testid="counter-modal">CounterModal</div> : null,
}));

const mockOnAccept = vi.fn();
const mockOnDecline = vi.fn();
const mockOnCounter = vi.fn();
const mockOnCancel = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const pendingOffer: Offer = {
  id: "offer-1",
  amount: 20000,
  listingPrice: 25000,
  percentageOff: 20,
  status: "Pending",
  expiresAt: "2025-12-31T00:00:00Z",
  initiatorId: "other-user",
};

const defaultProps = {
  offer: pendingOffer,
  currentUserId: "current-user",
  listingPrice: 25000,
  onAccept: mockOnAccept,
  onDecline: mockOnDecline,
  onCounter: mockOnCounter,
  onCancel: mockOnCancel,
};

describe("OfferCard", () => {
  it("renders offer amount and percentage", () => {
    render(<OfferCard {...defaultProps} />);
    expect(screen.getByText(/20000/)).toBeInTheDocument();
    expect(screen.getByText(/−20%/)).toBeInTheDocument();
  });

  it("shows accept/decline/counter buttons when user can respond", () => {
    render(<OfferCard {...defaultProps} />);
    expect(screen.getByText("offerCard.actions.accept")).toBeInTheDocument();
    expect(screen.getByText("offerCard.actions.decline")).toBeInTheDocument();
    expect(screen.getByText("offerCard.actions.counter")).toBeInTheDocument();
  });

  it("calls onAccept when accept button clicked", async () => {
    const user = userEvent.setup();
    render(<OfferCard {...defaultProps} />);
    await user.click(screen.getByText("offerCard.actions.accept"));
    expect(mockOnAccept).toHaveBeenCalledWith("offer-1");
  });

  it("calls onDecline when decline button clicked", async () => {
    const user = userEvent.setup();
    render(<OfferCard {...defaultProps} />);
    await user.click(screen.getByText("offerCard.actions.decline"));
    expect(mockOnDecline).toHaveBeenCalledWith("offer-1");
  });

  it("shows cancel button when user is initiator", () => {
    render(
      <OfferCard
        {...defaultProps}
        offer={{ ...pendingOffer, initiatorId: "current-user" }}
      />,
    );
    expect(screen.getByText("offerCard.actions.cancel")).toBeInTheDocument();
    expect(
      screen.queryByText("offerCard.actions.accept"),
    ).not.toBeInTheDocument();
  });

  it("calls onCancel when cancel button clicked", async () => {
    const user = userEvent.setup();
    render(
      <OfferCard
        {...defaultProps}
        offer={{ ...pendingOffer, initiatorId: "current-user" }}
      />,
    );
    await user.click(screen.getByText("offerCard.actions.cancel"));
    expect(mockOnCancel).toHaveBeenCalledWith("offer-1");
  });

  it("hides action buttons for non-pending offers", () => {
    render(
      <OfferCard
        {...defaultProps}
        offer={{ ...pendingOffer, status: "Accepted" }}
      />,
    );
    expect(
      screen.queryByText("offerCard.actions.accept"),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText("offerCard.actions.cancel"),
    ).not.toBeInTheDocument();
  });
});
