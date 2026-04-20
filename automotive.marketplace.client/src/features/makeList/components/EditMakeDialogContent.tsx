import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getMakeByIdOptions } from "../api/getMakeByIdOptions";
import { MakeFormData } from "../types/MakeFormData";
import MakeForm from "./MakeForm";

type EditMakeDialogContentProps = {
  id: string;
  onSubmit: (formData: MakeFormData) => Promise<void>;
};

const EditMakeDialogContent = ({ id, onSubmit }: EditMakeDialogContentProps) => {
  const { data: makeQuery } = useSuspenseQuery(getMakeByIdOptions({ id }));
  const make = makeQuery.data;

  return (
    <div>
      <DialogHeader>
        <DialogTitle>Edit {make.name}</DialogTitle>
      </DialogHeader>
      <MakeForm make={{ name: make.name }} onSubmit={onSubmit} />
    </div>
  );
};

export default EditMakeDialogContent;
