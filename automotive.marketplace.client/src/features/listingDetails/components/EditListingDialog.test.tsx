import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import EditListingDialog from "./EditListingDialog";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

const { mockUpdateListingAsync } = vi.hoisted(() => ({
  mockUpdateListingAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("../api/useUpdateListing", () => ({
  useUpdateListing: () => ({ mutateAsync: mockUpdateListingAsync }),
}));

vi.mock("./EditListingForm", () => ({
  default: ({
    listing,
    onSubmit,
  }: {
    listing: GetListingByIdResponse;
    id: string;
    onSubmit: (data: unknown) => void;
  }) => (
    <div data-testid="edit-form">
      <span data-testid="form-price">{listing.price}</span>
      <button data-testid="submit-btn" onClick={() => onSubmit({ price: 999 })}>
        Submit
      </button>
    </div>
  ),
}));

const baseListing: GetListingByIdResponse = {
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
  images: [],
  defects: [],
};

describe("EditListingDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders trigger button", () => {
    render(<EditListingDialog listing={baseListing} id="listing-1" />);
    expect(screen.getByRole("button")).toBeInTheDocument();
  });

  it("opens dialog when trigger is clicked", () => {
    render(<EditListingDialog listing={baseListing} id="listing-1" />);
    fireEvent.click(screen.getByRole("button"));
    expect(screen.getByText("edit.editListing")).toBeInTheDocument();
  });

  it("renders EditListingForm with listing data when dialog is open", () => {
    render(<EditListingDialog listing={baseListing} id="listing-1" />);
    fireEvent.click(screen.getByRole("button"));
    expect(screen.getByTestId("edit-form")).toBeInTheDocument();
    expect(screen.getByTestId("form-price")).toHaveTextContent("25000");
  });

  it("calls updateListingAsync on form submit", async () => {
    mockUpdateListingAsync.mockResolvedValue({});
    render(<EditListingDialog listing={baseListing} id="listing-1" />);
    fireEvent.click(screen.getByRole("button"));
    fireEvent.click(screen.getByTestId("submit-btn"));
    expect(mockUpdateListingAsync).toHaveBeenCalledWith({
      price: 999,
      id: "listing-1",
    });
  });
});
