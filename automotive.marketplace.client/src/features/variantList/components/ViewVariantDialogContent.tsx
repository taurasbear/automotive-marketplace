import {
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Variant } from "../types/Variant";

type ViewVariantDialogContentProps = {
  variant: Variant;
};

const ViewVariantDialogContent = ({
  variant,
}: ViewVariantDialogContentProps) => {
  return (
    <>
      <DialogHeader>
        <DialogTitle>Variant details</DialogTitle>
        <DialogDescription>Read-only view</DialogDescription>
      </DialogHeader>
      <div className="grid gap-4">
        <p>Year: {variant.year}</p>
        <p>Fuel: {variant.fuelName}</p>
        <p>Transmission: {variant.transmissionName}</p>
        <p>Body type: {variant.bodyTypeName}</p>
        <p>Doors: {variant.doorCount}</p>
        <p>Power: {variant.powerKw} kW</p>
        <p>Engine size: {variant.engineSizeMl} ml</p>
        <p>Custom: {variant.isCustom ? "Yes" : "No"}</p>
      </div>
    </>
  );
};

export default ViewVariantDialogContent;
