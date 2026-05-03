import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Inbox from "./Inbox";

const { mockNavigate } = vi.hoisted(() => ({
  mockNavigate: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
}));

vi.mock("@/components/ui/shadcn-io/spinner", () => ({
  Spinner: () => <div data-testid="spinner">Loading...</div>,
}));

vi.mock("@/features/chat", () => ({
  ConversationList: ({
    selectedId,
    onSelect,
  }: {
    selectedId: string | null;
    onSelect: (c: { id: string; listingTitle: string }) => void;
    initialConversationId?: string;
    onInitialLoad?: (c: null) => void;
  }) => (
    <div data-testid="conversation-list">
      <span data-testid="selected-id">{selectedId ?? "none"}</span>
      <button
        onClick={() =>
          onSelect({
            id: "conv-1",
            listingTitle: "BMW 320d",
          } as never)
        }
      >
        Select Conversation
      </button>
    </div>
  ),
  MessageThread: ({ conversation }: { conversation: { id: string } }) => (
    <div data-testid="message-thread">Thread: {conversation.id}</div>
  ),
}));

describe("Inbox", () => {
  beforeEach(() => {
    mockNavigate.mockReset();
  });

  it("renders split layout with aside and main", () => {
    render(<Inbox />);
    expect(screen.getByText("inbox.title")).toBeInTheDocument();
    expect(screen.getByTestId("conversation-list")).toBeInTheDocument();
  });

  it("shows empty state when no conversation is selected", () => {
    render(<Inbox />);
    expect(screen.getByText("inbox.emptyState")).toBeInTheDocument();
  });

  it("does not show MessageThread when no conversation is selected", () => {
    render(<Inbox />);
    expect(screen.queryByTestId("message-thread")).not.toBeInTheDocument();
  });

  it("shows MessageThread after selecting a conversation", async () => {
    const user = userEvent.setup();
    render(<Inbox />);
    await user.click(screen.getByText("Select Conversation"));
    expect(screen.getByTestId("message-thread")).toBeInTheDocument();
    expect(screen.getByText("Thread: conv-1")).toBeInTheDocument();
  });

  it("navigates to conversation route on selection", async () => {
    const user = userEvent.setup();
    render(<Inbox />);
    await user.click(screen.getByText("Select Conversation"));
    expect(mockNavigate).toHaveBeenCalledWith({
      to: "/inbox/$conversationId",
      params: { conversationId: "conv-1" },
    });
  });

  it("passes initialConversationId as selectedId when provided", () => {
    render(<Inbox initialConversationId="conv-initial" />);
    expect(screen.getByTestId("selected-id")).toHaveTextContent("conv-initial");
  });
});
