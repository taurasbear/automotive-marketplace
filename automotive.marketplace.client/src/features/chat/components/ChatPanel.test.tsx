import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ChatPanel from "./ChatPanel";
import type { ConversationSummary } from "../types/ConversationSummary";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("./MessageThread", () => ({
  default: () => <div data-testid="message-thread">MessageThread</div>,
}));

const mockOnClose = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const conversation: ConversationSummary = {
  id: "conv-1",
  listingId: "listing-1",
  listingTitle: "BMW 3 Series",
  listingThumbnail: null,
  listingPrice: 25000,
  listingMake: "BMW",
  listingCommercialName: "320d",
  listingVin: null,
  listingMileage: 100000,
  counterpartId: "user-2",
  counterpartUsername: "JohnDoe",
  lastMessage: "Hello",
  lastMessageAt: "2025-01-01T00:00:00Z",
  unreadCount: 0,
  buyerId: "user-1",
  sellerId: "user-2",
  buyerHasEngaged: true,
};

describe("ChatPanel", () => {
  it("renders conversation header with counterpart username", () => {
    render(<ChatPanel conversation={conversation} onClose={mockOnClose} />);
    expect(screen.getByText("JohnDoe")).toBeInTheDocument();
  });

  it("renders listing title in header", () => {
    render(<ChatPanel conversation={conversation} onClose={mockOnClose} />);
    expect(screen.getByText("BMW 3 Series")).toBeInTheDocument();
  });

  it("renders close button", () => {
    render(<ChatPanel conversation={conversation} onClose={mockOnClose} />);
    expect(
      screen.getByRole("button", { name: "chatPanel.closeChat" }),
    ).toBeInTheDocument();
  });

  it("calls onClose when close button clicked", async () => {
    render(<ChatPanel conversation={conversation} onClose={mockOnClose} />);
    const closeBtn = screen.getByRole("button", {
      name: "chatPanel.closeChat",
    });
    closeBtn.click();
    expect(mockOnClose).toHaveBeenCalled();
  });

  it("calls onClose when Escape key pressed", () => {
    render(<ChatPanel conversation={conversation} onClose={mockOnClose} />);
    fireEvent.keyDown(window, { key: "Escape" });
    expect(mockOnClose).toHaveBeenCalled();
  });

  it("renders MessageThread in suspense", () => {
    render(<ChatPanel conversation={conversation} onClose={mockOnClose} />);
    expect(screen.getByTestId("message-thread")).toBeInTheDocument();
  });

  it("does not show listing title when not provided", () => {
    render(
      <ChatPanel
        conversation={{ ...conversation, listingTitle: "" }}
        onClose={mockOnClose}
      />,
    );
    expect(screen.queryByText("BMW 3 Series")).not.toBeInTheDocument();
  });
});
