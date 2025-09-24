import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { Pencil } from "lucide-react";
import { getModelByIdOptions } from "../api/getModelByIdOptions";
import { ModelFormData } from "../types/ModelFormData";
import ModelForm from "./ModelForm";

type ViewModelDialogProps = {
  id: string;
};

const EditModelDialog = ({ id }: ViewModelDialogProps) => {
  const { data: modelQuery } = useSuspenseQuery(getModelByIdOptions({ id }));

  const model = modelQuery.data;

  const handleSubmit = async (formData: ModelFormData) => {
    // edit mutation
  };

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit {model.name}</DialogTitle>
        </DialogHeader>
        <ModelForm
          model={{
            ...model,
            firstAppearanceDate: new Date(model.firstAppearanceDate),
          }}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default EditModelDialog;
