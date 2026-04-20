import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { VariantFormData } from "../types/VariantFormData";
import { Variant } from "../types/Variant";
import VariantForm from "./VariantForm";

type EditVariantDialogContentProps = {
  variant: Variant;
  makeId: string;
  onSubmit: (formData: VariantFormData) => Promise<void>;
};

const EditVariantDialogContent = ({
  variant,
  makeId,
  onSubmit,
}: EditVariantDialogContentProps) => {
  return (
    <div>
      <DialogHeader>
        <DialogTitle>Edit variant ({variant.year})</DialogTitle>
      </DialogHeader>
      <VariantForm
        variant={{
          makeId,
          modelId: variant.modelId,
          year: variant.year,
          fuelId: variant.fuelId,
          transmissionId: variant.transmissionId,
          bodyTypeId: variant.bodyTypeId,
          doorCount: variant.doorCount,
          powerKw: variant.powerKw,
          engineSizeMl: variant.engineSizeMl,
          isCustom: variant.isCustom,
        }}
        onSubmit={onSubmit}
      />
    </div>
  );
};

export default EditVariantDialogContent;
