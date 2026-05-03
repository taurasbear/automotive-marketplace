import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import ListingDetails from "./ListingDetails";

vi.mock("@/app/routes/listing/$id", () => ({
  Route: {
    useParams: () => ({ id: "listing-123" }),
  },
}));

vi.mock("@/features/listingDetails", () => ({
  ListingDetailsContent: ({ id }: { id: string }) => (
    <div data-testid="listing-details-content">{id}</div>
  ),
}));

describe("ListingDetails page", () => {
  it("renders without crashing", () => {
    render(<ListingDetails />);
    expect(screen.getByTestId("listing-details-content")).toBeInTheDocument();
  });

  it("extracts listing ID from route params and passes to content", () => {
    render(<ListingDetails />);
    expect(screen.getByTestId("listing-details-content")).toHaveTextContent(
      "listing-123",
    );
  });
});
