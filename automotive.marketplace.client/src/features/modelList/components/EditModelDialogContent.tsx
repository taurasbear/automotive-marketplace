import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getModelByIdOptions } from "../api/getModelByIdOptions";
import { ModelFormData } from "../types/ModelFormData";
import ModelForm from "./ModelForm";

type EditModelDialogContentProps = {
  id: string;
  onSubmit: (formData: ModelFormData) => Promise<void>;
};

const EditModelDialogContent = ({
  id,
  onSubmit,
}: EditModelDialogContentProps) => {
  const { data: modelQuery } = useSuspenseQuery(getModelByIdOptions({ id }));
  const model = modelQuery.data;
  return (
    <div>
      <DialogHeader>
        <DialogTitle>Edit {model.name}</DialogTitle>
      </DialogHeader>
      <ModelForm
        model={{
          ...model,
          firstAppearanceDate: new Date(model.firstAppearanceDate),
        }}
        onSubmit={onSubmit}
      />
    </div>
  );
};

export default EditModelDialogContent;
