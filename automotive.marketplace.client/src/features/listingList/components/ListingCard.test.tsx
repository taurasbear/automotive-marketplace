import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ListingCard from "./ListingCard";
import type { GetAllListingsResponse } from "../types/GetAllListingsResponse";

const { mockUseAppSelector, mockToggleLikeMutate, mockRouterNavigate } =
  vi.hoisted(() => ({
    mockUseAppSelector: vi.fn(),
    mockToggleLikeMutate: vi.fn(),
    mockRouterNavigate: vi.fn(),
  }));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
  useAppDispatch: () => vi.fn(),
}));

vi.mock("@/features/auth", () => ({
  selectUserId: "selectUserId",
}));

vi.mock("@/features/savedListings/api/useToggleLike", () => ({
  useToggleLike: () => ({ mutate: mockToggleLikeMutate }),
}));

vi.mock("@/lib/router", () => ({
  router: { navigate: (...args: unknown[]) => mockRouterNavigate(...args) },
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (v: number) => v.toString(),
  formatNumber: (v: number) => v.toString(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("./ListingCardBadge", () => ({
  default: ({ title, stat }: { title: string; stat: string }) => (
    <div data-testid={`badge-${title}`}>{stat}</div>
  ),
}));

vi.mock("@/components/gallery/ImageHoverGallery", () => ({
  default: () => <div data-testid="image-gallery" />,
}));

vi.mock("react-icons/io5", () => ({
  IoLocationOutline: () => <span>location-icon</span>,
  IoHeartOutline: () => <span data-testid="heart-outline">♡</span>,
  IoHeart: () => <span data-testid="heart-filled">♥</span>,
}));

vi.mock("react-icons/md", () => ({
  MdOutlineLocalGasStation: () => <span>fuel-icon</span>,
}));

vi.mock("react-icons/pi", () => ({
  PiEngine: () => <span>engine-icon</span>,
}));

vi.mock("react-icons/tb", () => ({
  TbManualGearbox: () => <span>gearbox-icon</span>,
}));

const createListing = (
  overrides: Partial<GetAllListingsResponse> = {},
): GetAllListingsResponse => ({
  id: "listing-1",
  isUsed: true,
  year: 2020,
  makeName: "Audi",
  modelName: "A4",
  mileage: 50000,
  price: 25000,
  engineSizeMl: 2000,
  powerKw: 150,
  fuelName: "Diesel",
  transmissionName: "Automatic",
  municipalityName: "Vilnius",
  description: "Great car",
  thumbnail: { url: "https://img.com/car.jpg", altText: "Audi A4" },
  isLiked: false,
  images: [{ url: "https://img.com/car.jpg", altText: "Audi A4" }],
  imageCount: 1,
  defectCount: 0,
  sellerId: "seller-1",
  ...overrides,
});

describe("ListingCard", () => {
  beforeEach(() => {
    mockUseAppSelector.mockReturnValue("user-1");
    mockToggleLikeMutate.mockReset();
    mockRouterNavigate.mockReset();
  });

  it("renders listing title with year, make, and model", () => {
    render(<ListingCard listing={createListing()} />);

    expect(screen.getByText("2020 Audi A4")).toBeInTheDocument();
  });

  it("renders price and mileage", () => {
    render(<ListingCard listing={createListing()} />);

    expect(screen.getByText("25000 €")).toBeInTheDocument();
    expect(screen.getByText("50000 km")).toBeInTheDocument();
  });

  it("renders used/new badge", () => {
    render(<ListingCard listing={createListing({ isUsed: true })} />);
    expect(screen.getByText("card.used")).toBeInTheDocument();

    const { unmount } = render(
      <ListingCard listing={createListing({ isUsed: false })} />,
    );
    expect(screen.getByText("card.new")).toBeInTheDocument();
    unmount();
  });

  it("shows like button when user is logged in and not the seller", () => {
    mockUseAppSelector.mockReturnValue("user-1");
    render(<ListingCard listing={createListing({ sellerId: "seller-1" })} />);

    expect(screen.getByTestId("heart-outline")).toBeInTheDocument();
  });

  it("does not show like button when user is not logged in", () => {
    mockUseAppSelector.mockReturnValue(null);
    render(<ListingCard listing={createListing()} />);

    expect(screen.queryByTestId("heart-outline")).not.toBeInTheDocument();
    expect(screen.queryByTestId("heart-filled")).not.toBeInTheDocument();
  });

  it("does not show like button when user is the seller", () => {
    mockUseAppSelector.mockReturnValue("seller-1");
    render(<ListingCard listing={createListing({ sellerId: "seller-1" })} />);

    expect(screen.queryByTestId("heart-outline")).not.toBeInTheDocument();
  });

  it("shows filled heart when listing is liked", () => {
    mockUseAppSelector.mockReturnValue("user-1");
    render(<ListingCard listing={createListing({ isLiked: true })} />);

    expect(screen.getByTestId("heart-filled")).toBeInTheDocument();
  });

  it("calls toggleLike when like button is clicked", async () => {
    const user = userEvent.setup();
    mockUseAppSelector.mockReturnValue("user-1");
    render(<ListingCard listing={createListing({ id: "listing-99" })} />);

    const likeButton = screen.getByTestId("heart-outline").closest("button")!;
    await user.click(likeButton);

    expect(mockToggleLikeMutate).toHaveBeenCalledWith({
      listingId: "listing-99",
    });
  });

  it("navigates to listing detail on checkout button click", async () => {
    const user = userEvent.setup();
    mockRouterNavigate.mockResolvedValue(undefined);
    render(<ListingCard listing={createListing({ id: "listing-42" })} />);

    await user.click(screen.getByText("card.checkOut"));

    expect(mockRouterNavigate).toHaveBeenCalledWith({
      to: "/listing/$id",
      params: { id: "listing-42" },
    });
  });

  it("renders spec badges", () => {
    render(
      <ListingCard
        listing={createListing({
          engineSizeMl: 3000,
          powerKw: 200,
          fuelName: "Petrol",
          transmissionName: "Manual",
          municipalityName: "Kaunas",
        })}
      />,
    );

    expect(screen.getByTestId("badge-card.engine")).toHaveTextContent(
      "3 l 200 kW",
    );
    expect(screen.getByTestId("badge-card.location")).toHaveTextContent(
      "Kaunas",
    );
  });
});
