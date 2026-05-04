import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import MyListingsPage from "./MyListingsPage";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("@/features/chat", () => ({
  ChatPanel: ({ onClose }: { onClose: () => void }) => (
    <div data-testid="chat-panel">
      <button onClick={onClose}>Close</button>
    </div>
  ),
}));

vi.mock("./MyListingCard", () => ({
  default: ({ listing }: { listing: { id: string; makeName: string } }) => (
    <div data-testid={`listing-card-${listing.id}`}>{listing.makeName}</div>
  ),
}));

vi.mock("../api/getMyListingsOptions", () => ({
  getMyListingsOptions: {
    queryKey: ["myListings", "all"],
    queryFn: () => Promise.resolve({ data: [] }),
  },
}));

const createWrapper = (listings: unknown[] = []) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["myListings", "all"], { data: listings });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("MyListingsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders page title and create button", () => {
    render(<MyListingsPage />, { wrapper: createWrapper() });
    expect(screen.getByText("page.title")).toBeInTheDocument();
    expect(screen.getAllByText("page.createListing").length).toBeGreaterThan(0);
  });

  it("shows empty state when no listings", () => {
    render(<MyListingsPage />, { wrapper: createWrapper([]) });
    expect(screen.getByText("page.emptyState")).toBeInTheDocument();
    expect(screen.getByText("page.createFirst")).toBeInTheDocument();
  });

  it("renders MyListingCard for each listing", () => {
    const listings = [
      { id: "1", makeName: "BMW" },
      { id: "2", makeName: "Audi" },
    ];
    render(<MyListingsPage />, { wrapper: createWrapper(listings) });
    expect(screen.getByTestId("listing-card-1")).toBeInTheDocument();
    expect(screen.getByTestId("listing-card-2")).toBeInTheDocument();
    expect(screen.getByText("BMW")).toBeInTheDocument();
    expect(screen.getByText("Audi")).toBeInTheDocument();
  });

  it("shows loading skeletons when data is loading", () => {
    // Use a queryFn that never resolves to keep loading state
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <MyListingsPage />
      </QueryClientProvider>,
    );
    // The page title is NOT shown when loading (the loading skeleton branch renders)
    expect(screen.queryByText("page.title")).not.toBeInTheDocument();
  });

  it("create listing link points to /listing/create", () => {
    render(<MyListingsPage />, { wrapper: createWrapper() });
    const links = screen.getAllByRole("link");
    const createLink = links.find(
      (link) => link.getAttribute("href") === "/listing/create",
    );
    expect(createLink).toBeInTheDocument();
  });
});
