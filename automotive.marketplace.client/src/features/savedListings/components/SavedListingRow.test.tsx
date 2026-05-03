import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import SavedListingRow from "./SavedListingRow";
import type { SavedListing } from "../types/SavedListing";

const { mockToggleLikeMutate } = vi.hoisted(() => ({
  mockToggleLikeMutate: vi.fn(),
}));

vi.mock("../api/useToggleLike", () => ({
  useToggleLike: () => ({ mutate: mockToggleLikeMutate }),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (v: number) => v.toString(),
  formatNumber: (v: number) => v.toString(),
}));

vi.mock("@/features/listingList/utils/translateVehicleAttr", () => ({
  translateVehicleAttr: (_type: string, value: string) => value,
}));

vi.mock("react-icons/io5", () => ({
  IoHeart: () => <span data-testid="heart-icon">♥</span>,
}));

vi.mock("./NoteEditor", () => ({
  default: ({ listing }: { listing: SavedListing }) => (
    <div data-testid="note-editor">{listing.noteContent}</div>
  ),
}));

const createListing = (overrides: Partial<SavedListing> = {}): SavedListing => ({
  listingId: "listing-1",
  title: "2020 BMW X5",
  price: 35000,
  municipalityName: "Vilnius",
  mileage: 80000,
  fuelName: "Diesel",
  transmissionName: "Automatic",
  thumbnail: { url: "https://img.com/car.jpg", altText: "BMW X5" },
  noteContent: "A nice car",
  ...overrides,
});

describe("SavedListingRow", () => {
  beforeEach(() => {
    mockToggleLikeMutate.mockReset();
  });

  it("renders listing title", () => {
    render(<SavedListingRow listing={createListing()} />);
    expect(screen.getByText("2020 BMW X5")).toBeInTheDocument();
  });

  it("renders price, location, mileage, fuel, and transmission", () => {
    render(<SavedListingRow listing={createListing()} />);
    expect(screen.getByText(/35000 €/)).toBeInTheDocument();
    expect(screen.getByText(/Vilnius/)).toBeInTheDocument();
    expect(screen.getByText(/80000 km/)).toBeInTheDocument();
    expect(screen.getByText(/Diesel/)).toBeInTheDocument();
    expect(screen.getByText(/Automatic/)).toBeInTheDocument();
  });

  it("renders thumbnail image when available", () => {
    render(<SavedListingRow listing={createListing()} />);
    const img = screen.getByAltText("BMW X5");
    expect(img).toBeInTheDocument();
    expect(img).toHaveAttribute("src", "https://img.com/car.jpg");
  });

  it("renders no-image placeholder when thumbnail is null", () => {
    render(<SavedListingRow listing={createListing({ thumbnail: null })} />);
    expect(screen.getByText("row.noImage")).toBeInTheDocument();
  });

  it("calls toggleLike mutation when unlike button is clicked", async () => {
    const user = userEvent.setup();
    render(<SavedListingRow listing={createListing({ listingId: "listing-42" })} />);

    const unlikeButton = screen.getByTestId("heart-icon").closest("button")!;
    await user.click(unlikeButton);

    expect(mockToggleLikeMutate).toHaveBeenCalledWith({
      listingId: "listing-42",
    });
  });

  it("renders NoteEditor with listing", () => {
    render(<SavedListingRow listing={createListing({ noteContent: "Test note" })} />);
    expect(screen.getByTestId("note-editor")).toHaveTextContent("Test note");
  });

  it("links to listing detail page", () => {
    render(<SavedListingRow listing={createListing()} />);
    const link = screen.getByText("2020 BMW X5").closest("a");
    expect(link).toHaveAttribute("href", "/listing/$id");
  });
});
