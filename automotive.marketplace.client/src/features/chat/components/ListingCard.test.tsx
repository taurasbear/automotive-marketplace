import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import ListingCard from "./ListingCard";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (val: number) => String(val),
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({
    children,
    to,
    params,
  }: {
    children: React.ReactNode;
    to?: string;
    params?: Record<string, string>;
  }) => (
    <a href={`${to}/${params?.id ?? ""}`} data-testid="link">
      {children}
    </a>
  ),
}));

describe("ListingCard", () => {
  const defaultProps = {
    listingId: "listing-1",
    listingTitle: "BMW 3 Series",
    listingPrice: 25000,
    listingThumbnail: { url: "https://img.test/car.jpg", altText: "BMW" },
  };

  it("renders listing title and price", () => {
    render(<ListingCard {...defaultProps} />);
    expect(screen.getByText("BMW 3 Series")).toBeInTheDocument();
    expect(screen.getByText(/25000/)).toBeInTheDocument();
  });

  it("renders thumbnail when provided", () => {
    render(<ListingCard {...defaultProps} />);
    const img = screen.getByRole("img");
    expect(img).toHaveAttribute("src", "https://img.test/car.jpg");
    expect(img).toHaveAttribute("alt", "BMW");
  });

  it("does not render thumbnail when null", () => {
    render(<ListingCard {...defaultProps} listingThumbnail={null} />);
    expect(screen.queryByRole("img")).not.toBeInTheDocument();
  });

  it("renders view listing link", () => {
    render(<ListingCard {...defaultProps} />);
    expect(screen.getByText("listingCard.viewListing")).toBeInTheDocument();
  });
});
