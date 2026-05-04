import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import SavedListings from "./SavedListings";

vi.mock("@/features/savedListings/components/SavedListingsPage", () => ({
  default: () => <div data-testid="saved-listings-page">SavedListingsPage</div>,
}));

describe("SavedListings page", () => {
  it("renders SavedListingsPage component", () => {
    render(<SavedListings />);
    expect(screen.getByTestId("saved-listings-page")).toBeInTheDocument();
    expect(screen.getByText("SavedListingsPage")).toBeInTheDocument();
  });
});
