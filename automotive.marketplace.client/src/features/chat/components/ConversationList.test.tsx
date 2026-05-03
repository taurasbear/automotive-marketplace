import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense } from "react";
import ConversationList from "./ConversationList";
import type { ConversationSummary } from "../types/ConversationSummary";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

const conversations: ConversationSummary[] = [
  {
    id: "conv-1",
    listingId: "listing-1",
    listingTitle: "BMW 3 Series",
    listingThumbnail: { url: "https://img.test/car.jpg", altText: "BMW" },
    listingPrice: 25000,
    listingMake: "BMW",
    listingCommercialName: "320d",
    listingVin: null,
    listingMileage: 100000,
    counterpartId: "user-2",
    counterpartUsername: "JohnDoe",
    lastMessage: "Is it still available?",
    lastMessageAt: new Date().toISOString(),
    unreadCount: 3,
    buyerId: "user-1",
    sellerId: "user-2",
    buyerHasEngaged: true,
  },
  {
    id: "conv-2",
    listingId: "listing-2",
    listingTitle: "Audi A4",
    listingThumbnail: null,
    listingPrice: 20000,
    listingMake: "Audi",
    listingCommercialName: "A4",
    listingVin: null,
    listingMileage: 80000,
    counterpartId: "user-3",
    counterpartUsername: "JaneDoe",
    lastMessage: null,
    lastMessageAt: new Date().toISOString(),
    unreadCount: 0,
    buyerId: "user-1",
    sellerId: "user-3",
    buyerHasEngaged: true,
  },
];

vi.mock("../api/getConversationsOptions", () => ({
  getConversationsOptions: () => ({
    queryKey: ["chat", "conversations"],
    queryFn: () => Promise.resolve({ data: conversations }),
  }),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["chat", "conversations"], { data: conversations });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={<div>Loading...</div>}>{children}</Suspense>
    </QueryClientProvider>
  );
};

const createEmptyWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["chat", "conversations"], { data: [] });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={<div>Loading...</div>}>{children}</Suspense>
    </QueryClientProvider>
  );
};

const mockOnSelect = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

describe("ConversationList", () => {
  it("renders list of conversations", () => {
    render(
      <ConversationList selectedId={null} onSelect={mockOnSelect} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("BMW 3 Series")).toBeInTheDocument();
    expect(screen.getByText("Audi A4")).toBeInTheDocument();
  });

  it("renders counterpart username", () => {
    render(
      <ConversationList selectedId={null} onSelect={mockOnSelect} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("JohnDoe")).toBeInTheDocument();
    expect(screen.getByText("JaneDoe")).toBeInTheDocument();
  });

  it("renders last message preview", () => {
    render(
      <ConversationList selectedId={null} onSelect={mockOnSelect} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("Is it still available?")).toBeInTheDocument();
  });

  it("shows unread count badge", () => {
    render(
      <ConversationList selectedId={null} onSelect={mockOnSelect} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("calls onSelect when conversation clicked", async () => {
    const user = userEvent.setup();
    render(
      <ConversationList selectedId={null} onSelect={mockOnSelect} />,
      { wrapper: createWrapper() },
    );
    await user.click(screen.getByText("BMW 3 Series"));
    expect(mockOnSelect).toHaveBeenCalledWith(conversations[0]);
  });

  it("shows empty state when no conversations", () => {
    render(
      <ConversationList selectedId={null} onSelect={mockOnSelect} />,
      { wrapper: createEmptyWrapper() },
    );
    expect(
      screen.getByText("conversationList.noConversationsYet"),
    ).toBeInTheDocument();
  });

  it("renders listing thumbnail", () => {
    render(
      <ConversationList selectedId={null} onSelect={mockOnSelect} />,
      { wrapper: createWrapper() },
    );
    const img = screen.getByRole("img");
    expect(img).toHaveAttribute("src", "https://img.test/car.jpg");
  });
});
