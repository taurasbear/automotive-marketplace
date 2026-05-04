import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import MyListingCard from "./MyListingCard";
import type { GetMyListingsResponse } from "../types/GetMyListingsResponse";

const { mockUseAppSelector, mockDeleteMutate, mockUpdateStatusMutate, mockReactivateMutate } =
  vi.hoisted(() => ({
    mockUseAppSelector: vi.fn(),
    mockDeleteMutate: vi.fn(),
    mockUpdateStatusMutate: vi.fn(),
    mockReactivateMutate: vi.fn(),
  }));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (v: number) => v.toString(),
  formatNumber: (v: number) => v.toString(),
}));

vi.mock("@/features/listingList/utils/translateVehicleAttr", () => ({
  translateVehicleAttr: (_type: string, value: string) => value,
}));

vi.mock("@/features/listingList", () => ({
  ListingCardBadge: ({ title, stat }: { title: string; stat: string }) => (
    <div data-testid={`badge-${title}`}>{stat}</div>
  ),
}));

vi.mock("@/components/gallery/ImageHoverGallery", () => ({
  default: () => <div data-testid="image-gallery" />,
}));

vi.mock("react-icons/io5", () => ({
  IoLocationOutline: () => <span>location</span>,
}));

vi.mock("react-icons/md", () => ({
  MdOutlineLocalGasStation: () => <span>fuel</span>,
}));

vi.mock("react-icons/pi", () => ({
  PiEngine: () => <span>engine</span>,
}));

vi.mock("react-icons/tb", () => ({
  TbManualGearbox: () => <span>gearbox</span>,
}));

vi.mock("../api/useDeleteMyListing", () => ({
  useDeleteMyListing: () => ({
    mutate: mockDeleteMutate,
    isPending: false,
  }),
}));

vi.mock("../api/useUpdateListingStatus", () => ({
  useUpdateListingStatus: () => ({
    mutate: mockUpdateStatusMutate,
    isPending: false,
  }),
}));

vi.mock("../api/useReactivateListing", () => ({
  useReactivateListing: () => ({
    mutate: mockReactivateMutate,
    isPending: false,
  }),
}));

vi.mock("./ListingBuyerPanel", () => ({
  default: () => <div data-testid="buyer-panel">Buyer Panel</div>,
}));

vi.mock("./SellerInsightsPanel", () => ({
  SellerInsightsPanel: () => (
    <div data-testid="seller-insights">Insights</div>
  ),
}));

const createListing = (
  overrides: Partial<GetMyListingsResponse> = {},
): GetMyListingsResponse => ({
  id: "listing-1",
  price: 25000,
  mileage: 50000,
  isUsed: true,
  municipalityName: "Vilnius",
  status: "Available",
  year: 2020,
  makeName: "BMW",
  modelName: "X3",
  thumbnail: { url: "https://img.test/thumb.jpg", altText: "Thumb" },
  images: [{ url: "https://img.test/1.jpg", altText: "Front" }],
  imageCount: 1,
  defectCount: 0,
  fuelName: "Diesel",
  transmissionName: "Automatic",
  engineSizeMl: 2000,
  powerKw: 140,
  likeCount: 5,
  conversationCount: 2,
  ...overrides,
});

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("MyListingCard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAppSelector.mockReturnValue({ userId: "seller-1" });
  });

  it("renders card with year, make, model", () => {
    render(
      <MyListingCard listing={createListing()} onStartChat={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("2020 BMW X3")).toBeInTheDocument();
  });

  it("renders price and mileage", () => {
    render(
      <MyListingCard listing={createListing()} onStartChat={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("25000 €")).toBeInTheDocument();
    expect(screen.getByText("50000 km")).toBeInTheDocument();
  });

  it("renders status badge", () => {
    render(
      <MyListingCard listing={createListing()} onStartChat={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("card.active")).toBeInTheDocument();
  });

  it("shows sold status badge for sold listings", () => {
    render(
      <MyListingCard
        listing={createListing({ status: "Sold" })}
        onStartChat={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("card.sold")).toBeInTheDocument();
  });

  it("shows delete button and confirming calls delete mutation", () => {
    render(
      <MyListingCard listing={createListing()} onStartChat={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    // Click delete trigger button
    const deleteBtn = screen.getByTitle("card.delete");
    fireEvent.click(deleteBtn);

    // Confirm dialog appears
    expect(
      screen.getByText("detail.deleteConfirmTitle"),
    ).toBeInTheDocument();

    // Click confirm
    fireEvent.click(screen.getByText("detail.confirm"));
    expect(mockDeleteMutate).toHaveBeenCalledWith(
      { id: "listing-1" },
      expect.any(Object),
    );
  });

  it("shows mark as sold button for available listings", () => {
    render(
      <MyListingCard listing={createListing()} onStartChat={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("card.markAsSold")).toBeInTheDocument();
  });

  it("confirming mark as sold calls update status mutation", () => {
    render(
      <MyListingCard listing={createListing()} onStartChat={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    fireEvent.click(screen.getByText("card.markAsSold"));
    expect(
      screen.getByText("card.markAsSoldConfirmTitle"),
    ).toBeInTheDocument();
    fireEvent.click(screen.getByText("card.confirm"));
    expect(mockUpdateStatusMutate).toHaveBeenCalledWith(
      { id: "listing-1", status: "Bought" },
      expect.any(Object),
    );
  });

  it("shows reactivate button for OnHold listings", () => {
    render(
      <MyListingCard
        listing={createListing({ status: "OnHold" })}
        onStartChat={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("card.reactivate")).toBeInTheDocument();
  });

  it("hides action buttons for sold listings", () => {
    render(
      <MyListingCard
        listing={createListing({ status: "Sold" })}
        onStartChat={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );
    expect(screen.queryByTitle("card.delete")).not.toBeInTheDocument();
    expect(screen.queryByText("card.markAsSold")).not.toBeInTheDocument();
  });

  it("shows defect badge when defectCount > 0", () => {
    render(
      <MyListingCard
        listing={createListing({ defectCount: 3 })}
        onStartChat={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("card.defects")).toBeInTheDocument();
  });

  it("renders seller insights panel", () => {
    render(
      <MyListingCard listing={createListing()} onStartChat={vi.fn()} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByTestId("seller-insights")).toBeInTheDocument();
  });
});
