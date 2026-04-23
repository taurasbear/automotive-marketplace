import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useUpsertListingNote } from "../api/useUpsertListingNote";
import { useDeleteListingNote } from "../api/useDeleteListingNote";
import type { SavedListing } from "../types/SavedListing";
import PropertyMentionPicker from "./PropertyMentionPicker";

interface NoteEditorProps {
  listing: SavedListing;
  isExpanded: boolean;
}

const DEBOUNCE_MS = 800;

const NoteEditor = ({ listing, isExpanded }: NoteEditorProps) => {
  const { t } = useTranslation("saved");
  const [text, setText] = useState(listing.noteContent ?? "");
  const [isEditing, setIsEditing] = useState(false);
  const [showPicker, setShowPicker] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const saveTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const latestTextRef = useRef(text);

  const upsertNote = useUpsertListingNote();
  const deleteNote = useDeleteListingNote();

  const save = (value: string) => {
    const trimmed = value.trim();
    if (trimmed === "" && listing.noteContent) {
      deleteNote.mutate({ listingId: listing.listingId });
    } else if (trimmed !== "" && trimmed !== listing.noteContent) {
      upsertNote.mutate({ listingId: listing.listingId, content: trimmed });
    }
  };

  const scheduleSave = (value: string) => {
    latestTextRef.current = value;
    if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    saveTimerRef.current = setTimeout(() => save(value), DEBOUNCE_MS);
  };

  // Flush on unmount so navigating away doesn't lose unsaved text
  useEffect(() => {
    return () => {
      if (saveTimerRef.current) {
        clearTimeout(saveTimerRef.current);
        save(latestTextRef.current);
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleChipInsert = (chip: string) => {
    const textarea = textareaRef.current;
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const newText = text.slice(0, start) + chip + text.slice(end);
    setText(newText);

    setTimeout(() => {
      textarea.selectionStart = start + chip.length;
      textarea.selectionEnd = start + chip.length;
      textarea.focus();
    }, 0);
  };

  if (!isExpanded && !listing.noteContent) return null;

  if (!isExpanded && listing.noteContent) {
    return (
      <p className="text-muted-foreground truncate text-sm">
        {listing.noteContent}
      </p>
    );
  }

  return (
    <div className="relative mt-2">
      {isEditing ? (
        <div className="border-l-2 border-red-500 pl-3">
          <div className="relative">
            <textarea
              ref={textareaRef}
              className="w-full resize-none bg-transparent text-sm outline-none"
              value={text}
              onChange={(e) => {
                setText(e.target.value);
                scheduleSave(e.target.value);
              }}
              rows={3}
              autoFocus
            />
            <button
              className="text-muted-foreground hover:text-foreground absolute right-0 bottom-0 text-lg"
              onMouseDown={(e) => {
                e.preventDefault();
                setShowPicker(!showPicker);
              }}
            >
              +
            </button>
          </div>
          {showPicker && (
            <PropertyMentionPicker
              listing={listing}
              onSelect={handleChipInsert}
              onClose={() => setShowPicker(false)}
            />
          )}
        </div>
      ) : (
        <div
          className="cursor-pointer border-l-2 border-red-500 pl-3"
          onClick={() => setIsEditing(true)}
        >
          {listing.noteContent ? (
            <p className="text-sm">{listing.noteContent}</p>
          ) : (
            <p className="text-muted-foreground text-sm italic">
              {t("noteEditor.placeholder")}
            </p>
          )}
        </div>
      )}
    </div>
  );
};

export default NoteEditor;
