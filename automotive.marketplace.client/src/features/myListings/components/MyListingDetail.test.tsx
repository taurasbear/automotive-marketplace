import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense } from "react";
import MyListingDetail from "./MyListingDetail";

const { mockNavigate, mockDeleteAsync, mockUpdateAsync } = vi.hoisted(() => ({
  mockNavigate: vi.fn(),
  mockDeleteAsync: vi.fn(),
  mockUpdateAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: "lt" },
  }),
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
  useNavigate: () => mockNavigate,
}));

vi.mock("@/components/gallery/ImageArrowGallery", () => ({
  default: () => <div data-testid="image-gallery">Gallery</div>,
}));

vi.mock("@/components/forms/DefectSelector", () => ({
  default: () => <div data-testid="defect-selector">DefectSelector</div>,
}));

vi.mock("@/components/forms/select/LocationCombobox", () => ({
  default: ({ value }: { value: string }) => (
    <div data-testid="location-combobox">{value}</div>
  ),
}));

vi.mock("@/constants/uiConstants", () => ({
  UI_CONSTANTS: { SELECT: { ANY_LOCATION: { VALUE: "any" } } },
}));

vi.mock("@/lib/i18n/getTranslatedName", () => ({
  getTranslatedName: () => "TranslatedDefect",
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatNumber: (v: number) => v.toString(),
}));

vi.mock("@/features/listingList/utils/translateVehicleAttr", () => ({
  translateVehicleAttr: (_type: string, value: string) => value,
}));

vi.mock("@/features/listingDetails/api/useUpdateListing", () => ({
  useUpdateListing: () => ({
    mutateAsync: mockUpdateAsync,
    isPending: false,
  }),
}));

vi.mock("@/features/myListings/api/useDeleteMyListing", () => ({
  useDeleteMyListing: () => ({
    mutateAsync: mockDeleteAsync,
    isPending: false,
  }),
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
  modelName: "X5",
  price: 45000,
  powerKw: 200,
  engineSizeMl: 3000,
  mileage: 80000,
  isSteeringWheelRight: false,
  municipalityId: "mun-1",
  municipalityName: "Kaunas",
  isUsed: true,
  year: 2021,
  transmissionName: "Automatic",
  fuelName: "Diesel",
  doorCount: 5,
  bodyTypeName: "SUV",
  drivetrainName: "AWD",
  sellerName: "Seller",
  sellerId: "seller-1",
  status: "Active",
  colour: "Black",
  vin: "WBA12345",
  description: "Great SUV",
  images: [{ url: "https://img.test/1.jpg", altText: "Front" }],
  defects: [
    {
      id: "d-1",
      defectCategoryId: "cat-1",
      defectCategoryName: "Scratch",
      customName: null,
      images: [{ url: "https://img.test/defect.jpg", altText: "Defect" }],
    },
  ],
  isLiked: false,
};

vi.mock("@/features/listingDetails/api/getListingByIdOptions", () => ({
  getListingByIdOptions: () => ({
    queryKey: ["listing", "listing-1"],
    queryFn: () => Promise.resolve({ data: mockListing }),
  }),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  queryClient.setQueryData(["listing", "listing-1"], { data: mockListing });
  queryClient.setQueryData(["defectCategories"], { data: [] });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={<div>Loading...</div>}>{children}</Suspense>
    </QueryClientProvider>
  );
};

describe("MyListingDetail", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockDeleteAsync.mockResolvedValue({});
    mockUpdateAsync.mockResolvedValue({});
    mockNavigate.mockResolvedValue(undefined);
  });

  it("renders listing title with year, make, model", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("2021 BMW X5")).toBeInTheDocument();
  });

  it("renders listing price", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("45000 €")).toBeInTheDocument();
  });

  it("renders status badge", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("renders image gallery", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByTestId("image-gallery")).toBeInTheDocument();
  });

  it("renders vehicle specs (make, model, fuel, etc.)", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("vehicleInfo.title")).toBeInTheDocument();
    expect(screen.getByText("BMW")).toBeInTheDocument();
    expect(screen.getByText("X5")).toBeInTheDocument();
    expect(screen.getByText("Diesel")).toBeInTheDocument();
  });

  it("renders editable fields section", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(
      screen.getByText("vehicleInfo.editableTitle"),
    ).toBeInTheDocument();
    expect(screen.getByText("fields.price")).toBeInTheDocument();
    expect(screen.getByText("fields.mileage")).toBeInTheDocument();
  });

  it("renders delete button that shows confirmation dialog", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    fireEvent.click(screen.getByText("detail.deleteListing"));
    expect(
      screen.getByText("detail.deleteConfirmTitle"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("detail.deleteConfirmDescription"),
    ).toBeInTheDocument();
  });

  it("confirming delete calls delete mutation", async () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    fireEvent.click(screen.getByText("detail.deleteListing"));
    fireEvent.click(screen.getByText("detail.confirm"));
    expect(mockDeleteAsync).toHaveBeenCalledWith({ id: "listing-1" });
  });

  it("renders defect section", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("detail.defects")).toBeInTheDocument();
    expect(screen.getByTestId("defect-selector")).toBeInTheDocument();
  });

  it("renders back to my-listings link", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByText("detail.backToMyListings")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /detail.backToMyListings/ }),
    ).toHaveAttribute("href", "/my-listings");
  });

  it("renders location combobox", () => {
    render(<MyListingDetail id="listing-1" />, { wrapper: createWrapper() });
    expect(screen.getByTestId("location-combobox")).toBeInTheDocument();
  });
});
