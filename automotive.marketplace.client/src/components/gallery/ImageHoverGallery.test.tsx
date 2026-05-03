import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

import ImageHoverGallery from "./ImageHoverGallery";

const mockImages = [
  { url: "https://example.com/img1.jpg", altText: "Image 1" },
  { url: "https://example.com/img2.jpg", altText: "Image 2" },
  { url: "https://example.com/img3.jpg", altText: "Image 3" },
];

describe("ImageHoverGallery", () => {
  it("renders fallback when no images provided", () => {
    render(<ImageHoverGallery images={[]} />);
    const img = screen.getByAltText("gallery.noImageAvailable");
    expect(img).toBeInTheDocument();
  });

  it("renders custom fallback URL when provided", () => {
    render(
      <ImageHoverGallery images={[]} fallbackUrl="https://example.com/fallback.jpg" />,
    );
    const img = screen.getByAltText("gallery.noImageAvailable");
    expect(img).toHaveAttribute("src", "https://example.com/fallback.jpg");
  });

  it("renders first image by default", () => {
    render(<ImageHoverGallery images={mockImages} />);
    const img = screen.getByAltText("Image 1");
    expect(img).toBeInTheDocument();
    expect(img).toHaveAttribute("src", "https://example.com/img1.jpg");
  });

  it("changes image based on mouse position", () => {
    render(<ImageHoverGallery images={mockImages} />);
    const container = screen.getByAltText("Image 1").parentElement!;

    Object.defineProperty(container, "getBoundingClientRect", {
      value: () => ({ left: 0, width: 300, top: 0, height: 200 }),
    });

    fireEvent.mouseMove(container, { clientX: 250 });
    expect(screen.getByAltText("Image 3")).toBeInTheDocument();
  });

  it("resets to first image on mouse leave", () => {
    render(<ImageHoverGallery images={mockImages} />);
    const container = screen.getByAltText("Image 1").parentElement!;

    Object.defineProperty(container, "getBoundingClientRect", {
      value: () => ({ left: 0, width: 300, top: 0, height: 200 }),
    });

    fireEvent.mouseMove(container, { clientX: 250 });
    expect(screen.getByAltText("Image 3")).toBeInTheDocument();

    fireEvent.mouseLeave(container);
    expect(screen.getByAltText("Image 1")).toBeInTheDocument();
  });

  it("renders dot indicators for multiple images", () => {
    const { container } = render(<ImageHoverGallery images={mockImages} />);
    const dots = container.querySelectorAll(".rounded-full");
    expect(dots.length).toBe(3);
  });

  it("does not render dot indicators for single image", () => {
    const { container } = render(
      <ImageHoverGallery images={[mockImages[0]]} />,
    );
    const dots = container.querySelectorAll(".rounded-full");
    expect(dots.length).toBe(0);
  });
});
