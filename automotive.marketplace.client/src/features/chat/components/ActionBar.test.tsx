import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ActionBar from "./ActionBar";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("./MakeOfferModal", () => ({
  default: ({ open }: { open: boolean }) =>
    open ? <div data-testid="offer-modal">OfferModal</div> : null,
}));

vi.mock("./ProposeMeetingModal", () => ({
  default: ({ open }: { open: boolean }) =>
    open ? <div data-testid="meeting-modal">MeetingModal</div> : null,
}));

vi.mock("./ShareAvailabilityModal", () => ({
  default: ({ open }: { open: boolean }) =>
    open ? <div data-testid="availability-modal">AvailModal</div> : null,
}));

const mockOnSendOffer = vi.fn();
const mockOnProposeMeeting = vi.fn();
const mockOnShareAvailability = vi.fn();
const mockOnCancelMeeting = vi.fn();
const mockOnRequestContract = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const defaultProps = {
  currentUserId: "buyer-1",
  buyerId: "buyer-1",
  sellerId: "seller-1",
  listingPrice: 25000,
  conversationId: "conv-1",
  buyerHasEngaged: true,
  hasActiveOffer: false,
  hasActiveMeeting: false,
  acceptedMeeting: null,
  onSendOffer: mockOnSendOffer,
  onProposeMeeting: mockOnProposeMeeting,
  onShareAvailability: mockOnShareAvailability,
  onCancelMeeting: mockOnCancelMeeting,
  hasActiveContract: false,
  onRequestContract: mockOnRequestContract,
};

describe("ActionBar", () => {
  it("renders the plus button for buyer", () => {
    render(<ActionBar {...defaultProps} />);
    expect(screen.getByRole("button")).toBeInTheDocument();
  });

  it("renders nothing when seller and buyer has not engaged", () => {
    const { container } = render(
      <ActionBar
        {...defaultProps}
        currentUserId="seller-1"
        buyerHasEngaged={false}
      />,
    );
    expect(container.innerHTML).toBe("");
  });

  it("shows action menu items when popover is opened", async () => {
    const user = userEvent.setup();
    render(<ActionBar {...defaultProps} />);
    await user.click(screen.getByRole("button"));
    expect(screen.getByText("actionBar.makeAnOffer")).toBeInTheDocument();
    expect(screen.getByText("actionBar.proposeATime")).toBeInTheDocument();
    expect(screen.getByText("actionBar.shareAvailability")).toBeInTheDocument();
    expect(screen.getByText("actionBar.requestContract")).toBeInTheDocument();
  });

  it("opens offer modal when make an offer clicked", async () => {
    const user = userEvent.setup();
    render(<ActionBar {...defaultProps} />);
    await user.click(screen.getByRole("button"));
    await user.click(screen.getByText("actionBar.makeAnOffer"));
    expect(screen.getByTestId("offer-modal")).toBeInTheDocument();
  });

  it("disables offer button when hasActiveOffer", async () => {
    const user = userEvent.setup();
    render(<ActionBar {...defaultProps} hasActiveOffer={true} />);
    await user.click(screen.getByRole("button"));
    const offerBtn = screen.getByText("actionBar.makeAnOffer");
    expect(offerBtn.closest("button")).toBeDisabled();
  });

  it("disables meeting buttons when hasActiveMeeting", async () => {
    const user = userEvent.setup();
    render(<ActionBar {...defaultProps} hasActiveMeeting={true} />);
    await user.click(screen.getByRole("button"));
    const meetingBtn = screen.getByText("actionBar.proposeATime");
    expect(meetingBtn.closest("button")).toBeDisabled();
  });

  it("disables contract button when hasActiveContract", async () => {
    const user = userEvent.setup();
    render(<ActionBar {...defaultProps} hasActiveContract={true} />);
    await user.click(screen.getByRole("button"));
    const contractBtn = screen.getByText("actionBar.requestContract");
    expect(contractBtn.closest("button")).toBeDisabled();
  });

  it("calls onRequestContract when contract button clicked", async () => {
    const user = userEvent.setup();
    render(<ActionBar {...defaultProps} />);
    await user.click(screen.getByRole("button"));
    await user.click(screen.getByText("actionBar.requestContract"));
    expect(mockOnRequestContract).toHaveBeenCalled();
  });
});
