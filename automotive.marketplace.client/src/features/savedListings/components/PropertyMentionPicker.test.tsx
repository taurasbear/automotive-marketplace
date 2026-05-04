import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import PropertyMentionPicker from "./PropertyMentionPicker";
import type { SavedListing } from "../types/SavedListing";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (v: number) => v.toString(),
  formatNumber: (v: number) => v.toString(),
}));

const createListing = (overrides: Partial<SavedListing> = {}): SavedListing => ({
  listingId: "listing-1",
  title: "2020 BMW X5",
  price: 35000,
  municipalityName: "Vilnius",
  mileage: 80000,
  fuelName: "Diesel",
  transmissionName: "Automatic",
  thumbnail: null,
  noteContent: null,
  ...overrides,
});

describe("PropertyMentionPicker", () => {
  it("renders all property options", () => {
    const onSelect = vi.fn();
    const onClose = vi.fn();

    render(
      <PropertyMentionPicker
        listing={createListing()}
        onSelect={onSelect}
        onClose={onClose}
      />,
    );

    expect(screen.getByText("propertyMention.mileage")).toBeInTheDocument();
    expect(screen.getByText("propertyMention.price")).toBeInTheDocument();
    expect(screen.getByText("propertyMention.fuel")).toBeInTheDocument();
    expect(screen.getByText("propertyMention.transmission")).toBeInTheDocument();
    expect(screen.getByText("propertyMention.city")).toBeInTheDocument();
  });

  it("displays formatted values next to labels", () => {
    const onSelect = vi.fn();
    const onClose = vi.fn();

    render(
      <PropertyMentionPicker
        listing={createListing({ mileage: 120000, price: 42000 })}
        onSelect={onSelect}
        onClose={onClose}
      />,
    );

    expect(screen.getByText("120000 km")).toBeInTheDocument();
    expect(screen.getByText("42000 €")).toBeInTheDocument();
  });

  it("calls onSelect with formatted chip and onClose when option clicked", async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();
    const onClose = vi.fn();

    render(
      <PropertyMentionPicker
        listing={createListing({ municipalityName: "Kaunas" })}
        onSelect={onSelect}
        onClose={onClose}
      />,
    );

    const cityButton = screen.getByText("propertyMention.city").closest("button")!;
    await user.click(cityButton);

    expect(onSelect).toHaveBeenCalledWith("📌 propertyMention.city · Kaunas");
    expect(onClose).toHaveBeenCalled();
  });

  it("calls onSelect with mileage chip when mileage option clicked", async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();
    const onClose = vi.fn();

    render(
      <PropertyMentionPicker
        listing={createListing({ mileage: 50000 })}
        onSelect={onSelect}
        onClose={onClose}
      />,
    );

    const mileageButton = screen.getByText("propertyMention.mileage").closest("button")!;
    await user.click(mileageButton);

    expect(onSelect).toHaveBeenCalledWith("📌 propertyMention.mileage · 50000 km");
    expect(onClose).toHaveBeenCalled();
  });
});
