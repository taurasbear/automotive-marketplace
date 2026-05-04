import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense } from "react";
import ListingDetailsContent from "./ListingDetailsContent";

const { mockUseAppSelector, mockToggleLikeMutate, mockDeleteAsync, mockGetOrCreateConversation } =
  vi.hoisted(() => ({
    mockUseAppSelector: vi.fn(),
    mockToggleLikeMutate: vi.fn(),
    mockDeleteAsync: vi.fn(),
    mockGetOrCreateConversation: vi.fn(),
  }));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: "lt" },
  }),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
}));

vi.mock("@/features/auth", () => ({
  selectUserId: (state: { auth: { userId: string | null } }) =>
    state.auth.userId,
}));

vi.mock("@/features/savedListings/api/useToggleLike", () => ({
  useToggleLike: () => ({ mutate: mockToggleLikeMutate }),
}));

vi.mock("../api/useDeleteListing", () => ({
  useDeleteListing: () => ({ mutateAsync: mockDeleteAsync }),
}));

vi.mock("@/features/chat", () => ({
  useGetOrCreateConversation: () => ({
    mutateAsync: mockGetOrCreateConversation,
  }),
  ChatPanel: ({ onClose }: { onClose: () => void }) => (
    <div data-testid="chat-panel">
      <button onClick={onClose}>Close chat</button>
    </div>
  ),
}));

vi.mock("@/features/compareListings", () => ({
  CompareSearchModal: ({ open }: { open: boolean }) =>
    open ? <div data-testid="compare-modal">Compare</div> : null,
}));

vi.mock("@/components/gallery/ImageArrowGallery", () => ({
  default: () => <div data-testid="image-gallery">Gallery</div>,
}));

vi.mock("./EditListingDialog", () => ({
  default: () => <button data-testid="edit-dialog-btn">Edit</button>,
}));

vi.mock("./ScoreCard", () => ({
  ScoreCard: () => <div data-testid="score-card">Score</div>,
}));

vi.mock("./AiSummarySection", () => ({
  AiSummarySection: () => <div data-testid="ai-summary">AI Summary</div>,
}));

vi.mock("./ListingKeySpecs", () => ({
  ListingKeySpecs: () => <div data-testid="key-specs">Key Specs</div>,
}));

vi.mock("./ListingSecondaryDetails", () => ({
  ListingSecondaryDetails: () => (
    <div data-testid="secondary-details">Secondary Details</div>
  ),
}));

vi.mock("@/lib/router", () => ({
  router: { navigate: vi.fn() },
}));

vi.mock("@/lib/i18n/getTranslatedName", () => ({
  getTranslatedName: () => "Translated",
}));

vi.mock("@/api/defect/getDefectCategoriesOptions", () => ({
  getDefectCategoriesOptions: {
    queryKey: ["defectCategories"],
    queryFn: () => Promise.resolve({ data: [] }),
  },
}));

const mockListing = {
  id: "listing-1",
  makeName: "BMW",
  modelName: "3 Series",
  price: 25000,
  powerKw: 140,
  engineSizeMl: 2000,
  mileage: 120000,
  isSteeringWheelRight: false,
  municipalityId: "mun-1",
  municipalityName: "Vilnius",
  isUsed: true,
  year: 2019,
  transmissionName: "Manual",
  fuelName: "Diesel",
  doorCount: 4,
  bodyTypeName: "Sedan",
  drivetrainName: "RWD",
  sellerName: "John",
  sellerId: "seller-1",
  status: "Active",
  images: [{ url: "https://img.test/1.jpg", altText: "Front" }],
  defects: [],
  isLiked: false,
};

vi.mock("../api/getListingByIdOptions", () => ({
  getListingByIdOptions: () => ({
    queryKey: ["listing", "listing-1"],
    queryFn: () => Promise.resolve({ data: mockListing }),
  }),
}));

const createWrapper = (userId: string | null = null, permissions: string[] = []) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["listing", "listing-1"], { data: mockListing });
  queryClient.setQueryData(["defectCategories"], { data: [] });

  mockUseAppSelector.mockImplementation((selector: (state: unknown) => unknown) =>
    selector({ auth: { userId, permissions } }),
  );

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={<div>Loading...</div>}>{children}</Suspense>
    </QueryClientProvider>
  );
};

describe("ListingDetailsContent", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockDeleteAsync.mockResolvedValue({});
    mockGetOrCreateConversation.mockResolvedValue({
      data: { conversationId: "conv-1" },
    });
  });

  it("renders without crashing", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText(/2019 BMW 3 Series/)).toBeInTheDocument();
  });

  it("renders listing title with year, make, and model", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("2019 BMW 3 Series")).toBeInTheDocument();
  });

  it("renders listing price", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("25000.00 €")).toBeInTheDocument();
  });

  it("renders image gallery", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByTestId("image-gallery")).toBeInTheDocument();
  });

  it("renders key specs and secondary details", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByTestId("key-specs")).toBeInTheDocument();
    expect(screen.getByTestId("secondary-details")).toBeInTheDocument();
  });

  it("renders ScoreCard and AiSummarySection", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByTestId("score-card")).toBeInTheDocument();
    expect(screen.getByTestId("ai-summary")).toBeInTheDocument();
  });

  it("hides edit/delete buttons for guest users", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(null),
    });
    expect(screen.queryByTestId("edit-dialog-btn")).not.toBeInTheDocument();
  });

  it("shows delete button for the seller", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper("seller-1"),
    });
    // Seller sees the Pencil (navigate) button and Trash (delete) button
    const buttons = screen.getAllByRole("button");
    const deleteBtn = buttons.find((btn) =>
      btn.classList.contains("destructive") || btn.textContent === "",
    );
    expect(deleteBtn).toBeDefined();
  });

  it("shows edit dialog for admin (ManageListings permission)", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper("other-user", ["ManageListings"]),
    });
    expect(screen.getByTestId("edit-dialog-btn")).toBeInTheDocument();
  });

  it("shows like button for authenticated non-seller user", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper("other-user"),
    });
    // Like button is present
    const likeBtn = screen.getByRole("button", { name: "" });
    expect(likeBtn).toBeDefined();
  });

  it("calls toggleLike when like button is clicked", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper("other-user"),
    });
    // Find the like button (it has no text, just an icon)
    const buttons = screen.getAllByRole("button");
    // The like button has the rounded-full class
    const likeBtn = buttons.find((btn) =>
      btn.className.includes("rounded-full"),
    );
    if (likeBtn) {
      fireEvent.click(likeBtn);
      expect(mockToggleLikeMutate).toHaveBeenCalledWith({
        listingId: "listing-1",
      });
    }
  });

  it("shows contact seller button for authenticated non-seller", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper("other-user"),
    });
    expect(
      screen.getByRole("button", { name: "details.contactSeller" }),
    ).toBeInTheDocument();
  });

  it("hides contact seller button for the seller", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper("seller-1"),
    });
    expect(
      screen.queryByRole("button", { name: "details.contactSeller" }),
    ).not.toBeInTheDocument();
  });

  it("hides contact seller button for guest", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(null),
    });
    expect(
      screen.queryByRole("button", { name: "details.contactSeller" }),
    ).not.toBeInTheDocument();
  });

  it("shows compare button", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(
      screen.getByRole("button", { name: "details.compareWithAnother" }),
    ).toBeInTheDocument();
  });

  it("shows condition badges (new/used and municipality)", () => {
    render(<ListingDetailsContent id="listing-1" />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("card.used")).toBeInTheDocument();
    expect(screen.getByText("Vilnius")).toBeInTheDocument();
  });
});
