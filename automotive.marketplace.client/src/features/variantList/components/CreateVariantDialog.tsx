import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useState } from "react";
import { useCreateVariant } from "../api/useCreateVariant";
import { VariantFormData } from "../types/VariantFormData";
import VariantForm from "./VariantForm";

type CreateVariantDialogProps = {
  modelId: string;
  makeId: string;
};

const CreateVariantDialog = ({ modelId, makeId }: CreateVariantDialogProps) => {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  const { mutateAsync: createVariantAsync } = useCreateVariant();

  const handleSubmit = async (formData: VariantFormData) => {
    await createVariantAsync({
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
        <Button>Add variant</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create new variant</DialogTitle>
        </DialogHeader>
        <VariantForm
          variant={{
            makeId,
            modelId,
            fuelId: "",
            transmissionId: "",
            bodyTypeId: "",
            doorCount: 4,
            powerKw: 100,
            engineSizeMl: 1600,
            isCustom: false,
          }}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default CreateVariantDialog;
