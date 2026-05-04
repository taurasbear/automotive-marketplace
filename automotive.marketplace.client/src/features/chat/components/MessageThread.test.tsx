import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense } from "react";
import MessageThread from "./MessageThread";
import type { ConversationSummary } from "../types/ConversationSummary";
import type { Message } from "../types/GetMessagesResponse";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("../utils/timezone", () => ({
  getTimezoneOffsetLabel: () => "UTC+2",
}));

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (selector: (s: unknown) => unknown) =>
    mockUseAppSelector(selector),
}));

const { mockSendMessage, mockMarkRead } = vi.hoisted(() => ({
  mockSendMessage: vi.fn(),
  mockMarkRead: vi.fn(),
}));

vi.mock("../api/useChatHub", () => ({
  useChatHub: () => ({
    sendMessage: mockSendMessage,
    sendOffer: vi.fn(),
    respondToOffer: vi.fn(),
    cancelOffer: vi.fn(),
    proposeMeeting: vi.fn(),
    respondToMeeting: vi.fn(),
    shareAvailability: vi.fn(),
    respondToAvailability: vi.fn(),
    cancelMeeting: vi.fn(),
    cancelAvailability: vi.fn(),
    requestContract: vi.fn(),
    respondToContract: vi.fn(),
    cancelContract: vi.fn(),
    submitContractSellerForm: vi.fn(),
    submitContractBuyerForm: vi.fn(),
  }),
}));

vi.mock("../api/useMarkMessagesRead", () => ({
  useMarkMessagesRead: () => ({ mutate: mockMarkRead }),
}));

vi.mock("@/lib/axios/axiosClient", () => ({
  default: { get: vi.fn() },
}));

vi.mock("../api/getMessagesOptions", () => ({
  getMessagesOptions: (query: { conversationId: string }) => ({
    queryKey: ["chat", "messages", query.conversationId],
    queryFn: () => Promise.resolve({ data: { conversationId: query.conversationId, messages: [] } }),
  }),
}));

vi.mock("./ListingCard", () => ({
  default: () => <div data-testid="listing-card">ListingCard</div>,
}));

vi.mock("./ActionBar", () => ({
  default: () => <div data-testid="action-bar">ActionBar</div>,
}));

vi.mock("./OfferCard", () => ({
  default: ({ offer }: { offer: { amount: number } }) => (
    <div data-testid="offer-card">Offer: {offer.amount}</div>
  ),
}));

vi.mock("./MeetingCard", () => ({
  default: () => <div data-testid="meeting-card">MeetingCard</div>,
}));

vi.mock("./AvailabilityCardComponent", () => ({
  default: () => <div data-testid="availability-card">AvailabilityCard</div>,
}));

vi.mock("./ContractCard", () => ({
  default: () => <div data-testid="contract-card">ContractCard</div>,
}));

vi.mock("./ContractFormDialog", () => ({
  default: () => null,
}));

const textMessages: Message[] = [
  {
    id: "msg-1",
    senderId: "user-1",
    senderUsername: "Me",
    content: "Hello there!",
    sentAt: "2025-01-01T10:00:00Z",
    isRead: true,
    messageType: "Text",
  },
  {
    id: "msg-2",
    senderId: "user-2",
    senderUsername: "JohnDoe",
    content: "Hi! How are you?",
    sentAt: "2025-01-01T10:01:00Z",
    isRead: true,
    messageType: "Text",
  },
];

const messagesWithOffer: Message[] = [
  ...textMessages,
  {
    id: "msg-3",
    senderId: "user-2",
    senderUsername: "JohnDoe",
    content: "",
    sentAt: "2025-01-01T10:02:00Z",
    isRead: true,
    messageType: "Offer",
    offer: {
      id: "offer-1",
      amount: 20000,
      listingPrice: 25000,
      percentageOff: 20,
      status: "Pending",
      expiresAt: "2025-12-31T00:00:00Z",
      initiatorId: "user-2",
    },
  },
];

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
  lastMessage: "Hi!",
  lastMessageAt: "2025-01-01T10:01:00Z",
  unreadCount: 0,
  buyerId: "user-1",
  sellerId: "user-2",
  buyerHasEngaged: true,
};

const createWrapper = (messages: Message[]) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["chat", "messages", "conv-1"], {
    data: { conversationId: "conv-1", messages },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={<div>Loading...</div>}>{children}</Suspense>
    </QueryClientProvider>
  );
};

beforeEach(() => {
  vi.clearAllMocks();
  mockUseAppSelector.mockReturnValue("user-1");
  // scrollIntoView is not implemented in jsdom
  Element.prototype.scrollIntoView = vi.fn();
});

describe("MessageThread", () => {
  it("renders text messages", () => {
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(screen.getByText("Hello there!")).toBeInTheDocument();
    expect(screen.getByText("Hi! How are you?")).toBeInTheDocument();
  });

  it("renders listing card when showListingCard is true", () => {
    render(<MessageThread conversation={conversation} showListingCard={true} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(screen.getByTestId("listing-card")).toBeInTheDocument();
  });

  it("does not render listing card when showListingCard is false", () => {
    render(<MessageThread conversation={conversation} showListingCard={false} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(screen.queryByTestId("listing-card")).not.toBeInTheDocument();
  });

  it("renders offer cards for offer messages", () => {
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(messagesWithOffer),
    });
    expect(screen.getByTestId("offer-card")).toBeInTheDocument();
  });

  it("renders action bar", () => {
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(screen.getByTestId("action-bar")).toBeInTheDocument();
  });

  it("renders input field with placeholder", () => {
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(
      screen.getByPlaceholderText("messageThread.placeholder"),
    ).toBeInTheDocument();
  });

  it("renders send button", () => {
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(screen.getByText("messageThread.send")).toBeInTheDocument();
  });

  it("send button is disabled when input is empty", () => {
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(screen.getByText("messageThread.send")).toBeDisabled();
  });

  it("calls sendMessage when send button clicked with input", async () => {
    const user = userEvent.setup();
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });

    const input = screen.getByPlaceholderText("messageThread.placeholder");
    await user.type(input, "New message");
    await user.click(screen.getByText("messageThread.send"));

    expect(mockSendMessage).toHaveBeenCalledWith({
      conversationId: "conv-1",
      content: "New message",
    });
  });

  it("calls sendMessage on Enter key", async () => {
    const user = userEvent.setup();
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });

    const input = screen.getByPlaceholderText("messageThread.placeholder");
    await user.type(input, "Enter message{enter}");

    expect(mockSendMessage).toHaveBeenCalledWith({
      conversationId: "conv-1",
      content: "Enter message",
    });
  });

  it("marks messages as read on mount", () => {
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(textMessages),
    });
    expect(mockMarkRead).toHaveBeenCalledWith("conv-1");
  });

  it("renders meeting card for meeting messages", () => {
    const meetingMessages: Message[] = [
      {
        id: "msg-m1",
        senderId: "user-2",
        senderUsername: "JohnDoe",
        content: "",
        sentAt: "2025-01-01T10:02:00Z",
        isRead: true,
        messageType: "Meeting",
        meeting: {
          id: "meeting-1",
          proposedAt: "2025-08-15T14:00:00Z",
          durationMinutes: 60,
          status: "Pending",
          expiresAt: "2025-08-14T14:00:00Z",
          initiatorId: "user-2",
        },
      },
    ];
    render(<MessageThread conversation={conversation} />, {
      wrapper: createWrapper(meetingMessages),
    });
    expect(screen.getByTestId("meeting-card")).toBeInTheDocument();
  });
});
