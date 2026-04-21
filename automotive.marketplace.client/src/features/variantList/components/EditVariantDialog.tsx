import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { useUpdateVariant } from "../api/useUpdateVariant";
import { Variant } from "../types/Variant";
import { VariantFormData } from "../types/VariantFormData";
import EditVariantDialogContent from "./EditVariantDialogContent";

type EditVariantDialogProps = {
  variant: Variant;
  makeId: string;
};

const EditVariantDialog = ({ variant, makeId }: EditVariantDialogProps) => {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  const { mutateAsync: updateVariantAsync } = useUpdateVariant();

  const handleSubmit = async (formData: VariantFormData) => {
    await updateVariantAsync({
      id: variant.id,
      modelId: formData.modelId,
      fuelId: formData.fuelId,
      transmissionId: formData.transmissionId,
      bodyTypeId: formData.bodyTypeId,
      doorCount: formData.doorCount,
      powerKw: formData.powerKw,
      engineSizeMl: formData.engineSizeMl,
      isCustom: formData.isCustom,
    });
    setIsOpen(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <EditVariantDialogContent
          variant={variant}
          makeId={makeId}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default EditVariantDialog;
