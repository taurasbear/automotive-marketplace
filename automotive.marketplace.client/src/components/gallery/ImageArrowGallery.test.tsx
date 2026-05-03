import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

import ImageArrowGallery from "./ImageArrowGallery";

const mockImages = [
  { url: "https://example.com/img1.jpg", altText: "Image 1" },
  { url: "https://example.com/img2.jpg", altText: "Image 2" },
  { url: "https://example.com/img3.jpg", altText: "Image 3", defectName: "Scratch" },
];

describe("ImageArrowGallery", () => {
  it("renders empty state when no images provided", () => {
    render(<ImageArrowGallery images={[]} />);
    expect(screen.getByText("gallery.noImagesAvailable")).toBeInTheDocument();
  });

  it("renders the first image and counter", () => {
    render(<ImageArrowGallery images={mockImages} />);
    const imgs = screen.getAllByAltText("Image 1");
    const mainImg = imgs.find((img) => img.className.includes("aspect-video"))!;
    expect(mainImg).toBeInTheDocument();
    expect(mainImg).toHaveAttribute("src", "https://example.com/img1.jpg");
    expect(screen.getByText("1 / 3")).toBeInTheDocument();
  });

  it("shows navigation arrows when more than one image", () => {
    render(<ImageArrowGallery images={mockImages} />);
    expect(screen.getByLabelText("gallery.previousImage")).toBeInTheDocument();
    expect(screen.getByLabelText("gallery.nextImage")).toBeInTheDocument();
  });

  it("does not show navigation arrows for single image", () => {
    render(<ImageArrowGallery images={[mockImages[0]]} />);
    expect(screen.queryByLabelText("gallery.previousImage")).not.toBeInTheDocument();
    expect(screen.queryByLabelText("gallery.nextImage")).not.toBeInTheDocument();
  });

  it("advances to next image on right arrow click", () => {
    const { container } = render(<ImageArrowGallery images={mockImages} />);
    fireEvent.click(screen.getByLabelText("gallery.nextImage"));
    const mainImg = container.querySelector("img.aspect-video")!;
    expect(mainImg).toHaveAttribute("alt", "Image 2");
    expect(screen.getByText("2 / 3")).toBeInTheDocument();
  });

  it("goes to previous image on left arrow click", () => {
    const { container } = render(<ImageArrowGallery images={mockImages} />);
    fireEvent.click(screen.getByLabelText("gallery.nextImage"));
    fireEvent.click(screen.getByLabelText("gallery.previousImage"));
    const mainImg = container.querySelector("img.aspect-video")!;
    expect(mainImg).toHaveAttribute("alt", "Image 1");
    expect(screen.getByText("1 / 3")).toBeInTheDocument();
  });

  it("wraps around to last image when pressing left on first image", () => {
    const { container } = render(<ImageArrowGallery images={mockImages} />);
    fireEvent.click(screen.getByLabelText("gallery.previousImage"));
    const mainImg = container.querySelector("img.aspect-video")!;
    expect(mainImg).toHaveAttribute("alt", "Image 3");
    expect(screen.getByText("3 / 3")).toBeInTheDocument();
  });

  it("handles keyboard ArrowRight navigation", () => {
    const { container } = render(<ImageArrowGallery images={mockImages} />);
    fireEvent.keyDown(window, { key: "ArrowRight" });
    const mainImg = container.querySelector("img.aspect-video")!;
    expect(mainImg).toHaveAttribute("alt", "Image 2");
  });

  it("handles keyboard ArrowLeft navigation", () => {
    const { container } = render(<ImageArrowGallery images={mockImages} />);
    fireEvent.keyDown(window, { key: "ArrowLeft" });
    const mainImg = container.querySelector("img.aspect-video")!;
    expect(mainImg).toHaveAttribute("alt", "Image 3");
  });

  it("shows defect info bar when current image has defectName", () => {
    const { container } = render(<ImageArrowGallery images={mockImages} />);
    fireEvent.click(screen.getByLabelText("gallery.nextImage"));
    fireEvent.click(screen.getByLabelText("gallery.nextImage"));
    const mainImg = container.querySelector("img.aspect-video")!;
    expect(mainImg).toHaveAttribute("alt", "Image 3");
    expect(screen.getByText("Scratch")).toBeInTheDocument();
    expect(screen.getByText("gallery.defectLabel")).toBeInTheDocument();
  });

  it("renders thumbnail strip for multiple images", () => {
    render(<ImageArrowGallery images={mockImages} />);
    const thumbnails = screen.getAllByRole("button").filter((btn) =>
      btn.querySelector("img"),
    );
    expect(thumbnails).toHaveLength(3);
  });
});
