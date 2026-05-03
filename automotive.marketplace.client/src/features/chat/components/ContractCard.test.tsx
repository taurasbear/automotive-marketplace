import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ContractCardComponent from "./ContractCard";
import type { ContractCard } from "../types/ContractCard";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

const mockOnAccept = vi.fn();
const mockOnDecline = vi.fn();
const mockOnCancel = vi.fn();
const mockOnFillOut = vi.fn();
const mockOnViewSubmitted = vi.fn();
const mockOnExportPdf = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const pendingCard: ContractCard = {
  id: "contract-1",
  status: "Pending",
  initiatorId: "other-user",
  acceptedAt: null,
  createdAt: "2025-01-01T00:00:00Z",
  sellerSubmittedAt: null,
  buyerSubmittedAt: null,
};

const defaultProps = {
  card: pendingCard,
  currentUserId: "current-user",
  isSeller: true,
  onAccept: mockOnAccept,
  onDecline: mockOnDecline,
  onCancel: mockOnCancel,
  onFillOut: mockOnFillOut,
  onViewSubmitted: mockOnViewSubmitted,
  onExportPdf: mockOnExportPdf,
};

describe("ContractCardComponent", () => {
  it("renders contract card with status header", () => {
    render(<ContractCardComponent {...defaultProps} />);
    expect(
      screen.getByText("contractCard.statusLabels.pending"),
    ).toBeInTheDocument();
  });

  it("shows accept/decline for pending recipient", () => {
    render(<ContractCardComponent {...defaultProps} />);
    expect(screen.getByText("contractCard.accept")).toBeInTheDocument();
    expect(screen.getByText("contractCard.decline")).toBeInTheDocument();
  });

  it("calls onAccept when accept button clicked", async () => {
    const user = userEvent.setup();
    render(<ContractCardComponent {...defaultProps} />);
    await user.click(screen.getByText("contractCard.accept"));
    expect(mockOnAccept).toHaveBeenCalledWith("contract-1");
  });

  it("calls onDecline when decline button clicked", async () => {
    const user = userEvent.setup();
    render(<ContractCardComponent {...defaultProps} />);
    await user.click(screen.getByText("contractCard.decline"));
    expect(mockOnDecline).toHaveBeenCalledWith("contract-1");
  });

  it("shows waiting message for pending initiator", () => {
    render(
      <ContractCardComponent
        {...defaultProps}
        card={{ ...pendingCard, initiatorId: "current-user" }}
      />,
    );
    expect(
      screen.getByText("contractCard.waitingForResponse"),
    ).toBeInTheDocument();
  });

  it("shows fill out button for Active status when not submitted", () => {
    render(
      <ContractCardComponent
        {...defaultProps}
        card={{ ...pendingCard, status: "Active" }}
      />,
    );
    expect(screen.getByText("contractCard.fillOut")).toBeInTheDocument();
  });

  it("calls onFillOut when fill out button clicked", async () => {
    const user = userEvent.setup();
    render(
      <ContractCardComponent
        {...defaultProps}
        card={{ ...pendingCard, status: "Active" }}
      />,
    );
    await user.click(screen.getByText("contractCard.fillOut"));
    expect(mockOnFillOut).toHaveBeenCalledWith("contract-1");
  });

  it("shows export PDF for Complete status", () => {
    render(
      <ContractCardComponent
        {...defaultProps}
        card={{
          ...pendingCard,
          status: "Complete",
          sellerSubmittedAt: "2025-01-02T00:00:00Z",
          buyerSubmittedAt: "2025-01-03T00:00:00Z",
        }}
      />,
    );
    expect(screen.getByText("contractCard.exportPdf")).toBeInTheDocument();
  });

  it("shows declined message for Declined status", () => {
    render(
      <ContractCardComponent
        {...defaultProps}
        card={{ ...pendingCard, status: "Declined" }}
      />,
    );
    expect(
      screen.getByText("contractCard.declinedMessage"),
    ).toBeInTheDocument();
  });

  it("shows cancelled message for Cancelled status", () => {
    render(
      <ContractCardComponent
        {...defaultProps}
        card={{ ...pendingCard, status: "Cancelled" }}
      />,
    );
    expect(
      screen.getByText("contractCard.cancelledMessage"),
    ).toBeInTheDocument();
  });
});
